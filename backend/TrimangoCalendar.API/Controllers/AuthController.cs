using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TrimangoCalendar.API.Contracts;
using TrimangoCalendar.Core.Entities;

namespace TrimangoCalendar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string TokenProvider = "TrimangoAuth";
    private const string RefreshTokenName = "RefreshToken";
    private const string RefreshTokenExpiryName = "RefreshTokenExpiryUtc";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponseDto<AuthTokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
        {
            return BadRequest(new ApiErrorResponseDto { Success = false, Message = "Bu e-posta ile kayıtlı kullanıcı zaten var." });
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            TenantId = dto.TenantId,
            IsActive = true,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(new ApiErrorResponseDto
            {
                Success = false,
                Message = string.Join(" | ", createResult.Errors.Select(e => e.Description))
            });
        }

        var tokenResponse = await CreateAndPersistTokensAsync(user);
        return Ok(new ApiResponseDto<AuthTokenResponseDto>
        {
            Success = true,
            Data = tokenResponse,
            Message = "Kayıt işlemi başarılı."
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponseDto<AuthTokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.IsActive)
        {
            return Unauthorized(new ApiErrorResponseDto { Success = false, Message = "Kullanıcı bulunamadı veya pasif." });
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
        {
            return Unauthorized(new ApiErrorResponseDto { Success = false, Message = "E-posta veya şifre hatalı." });
        }

        var tokenResponse = await CreateAndPersistTokensAsync(user);
        return Ok(new ApiResponseDto<AuthTokenResponseDto>
        {
            Success = true,
            Data = tokenResponse,
            Message = "Giriş başarılı."
        });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponseDto<ForgotPasswordResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return Ok(new ApiResponseDto<ForgotPasswordResponseDto>
            {
                Success = true,
                Data = new ForgotPasswordResponseDto { Message = "Eğer hesap varsa şifre sıfırlama süreci başlatıldı." }
            });
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        return Ok(new ApiResponseDto<ForgotPasswordResponseDto>
        {
            Success = true,
            Data = new ForgotPasswordResponseDto
            {
                Message = "Şifre sıfırlama tokenı üretildi.",
                ResetToken = resetToken
            }
        });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return BadRequest(new ApiErrorResponseDto { Success = false, Message = "Kullanıcı bulunamadı." });
        }

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(new ApiErrorResponseDto
            {
                Success = false,
                Message = string.Join(" | ", result.Errors.Select(e => e.Description))
            });
        }

        return Ok(new ApiResponseDto<object>
        {
            Success = true,
            Message = "Şifre başarıyla güncellendi."
        });
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponseDto<AuthTokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        var principal = GetPrincipalFromExpiredToken(dto.AccessToken);
        var userIdClaim = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new ApiErrorResponseDto { Success = false, Message = "Geçersiz access token." });
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.IsActive)
        {
            return Unauthorized(new ApiErrorResponseDto { Success = false, Message = "Kullanıcı bulunamadı veya pasif." });
        }

        var storedRefreshToken = await _userManager.GetAuthenticationTokenAsync(user, TokenProvider, RefreshTokenName);
        var storedRefreshExpiry = await _userManager.GetAuthenticationTokenAsync(user, TokenProvider, RefreshTokenExpiryName);

        if (string.IsNullOrWhiteSpace(storedRefreshToken) ||
            !string.Equals(storedRefreshToken, dto.RefreshToken, StringComparison.Ordinal))
        {
            return Unauthorized(new ApiErrorResponseDto { Success = false, Message = "Geçersiz refresh token." });
        }

        if (!DateTime.TryParse(storedRefreshExpiry, out var refreshExpiryUtc) || refreshExpiryUtc <= DateTime.UtcNow)
        {
            return Unauthorized(new ApiErrorResponseDto { Success = false, Message = "Refresh token süresi dolmuş." });
        }

        var response = await CreateAndPersistTokensAsync(user);
        return Ok(new ApiResponseDto<AuthTokenResponseDto>
        {
            Success = true,
            Data = response,
            Message = "Token yenileme başarılı."
        });
    }

    private async Task<AuthTokenResponseDto> CreateAndPersistTokensAsync(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey ayarı bulunamadı.");
        var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer ayarı bulunamadı.");
        var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience ayarı bulunamadı.");
        var accessTokenMinutes = jwtSettings.GetValue<int?>("AccessTokenExpirationMinutes") ?? 60;
        var refreshTokenDays = jwtSettings.GetValue<int?>("RefreshTokenExpirationDays") ?? 7;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(accessTokenMinutes);

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty)
        };

        if (user.TenantId.HasValue)
        {
            claims.Add(new Claim("TenantId", user.TenantId.Value.ToString()));
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays);

        await _userManager.SetAuthenticationTokenAsync(user, TokenProvider, RefreshTokenName, refreshToken);
        await _userManager.SetAuthenticationTokenAsync(user, TokenProvider, RefreshTokenExpiryName, refreshTokenExpiresAt.ToString("O"));

        return new AuthTokenResponseDto
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = expiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey ayarı bulunamadı.");
        var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer ayarı bulunamadı.");
        var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience ayarı bulunamadı.");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Geçersiz token.");
        }

        return principal;
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
