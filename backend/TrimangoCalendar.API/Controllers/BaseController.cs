using Microsoft.AspNetCore.Mvc;
using TrimangoCalendar.API.Swagger;

namespace TrimangoCalendar.API.Controllers;

/// <summary>
/// API controller'ları için ortak yardımcı metotları sağlar.
/// </summary>
[Produces("application/json")]
[ProducesResponseType(typeof(SwaggerSuccessResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(SwaggerErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(SwaggerErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(SwaggerErrorResponse), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(SwaggerErrorResponse), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(SwaggerErrorResponse), StatusCodes.Status500InternalServerError)]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Request context içinden tenant kimliğini döner.
    /// </summary>
    protected Guid GetTenantId()
    {
        if (HttpContext.Items["TenantId"] is Guid tenantId)
        {
            return tenantId;
        }

        if (User?.Identity?.IsAuthenticated == true &&
            Guid.TryParse(User.FindFirst("TenantId")?.Value, out var claimTenantId))
        {
            return claimTenantId;
        }

        throw new UnauthorizedAccessException("Tenant bilgisi bulunamadı.");
    }

    /// <summary>
    /// Başarılı bir sonucu standart formatta döner.
    /// </summary>
    protected IActionResult Success(object? data = null, string? message = null)
    {
        return Ok(new
        {
            success = true,
            data,
            message
        });
    }

    /// <summary>
    /// Başarılı oluşturma sonucunu standart formatta döner.
    /// </summary>
    protected IActionResult CreatedSuccess(string actionName, object routeValues, object? data = null, string? message = null)
    {
        return CreatedAtAction(actionName, routeValues, new
        {
            success = true,
            data,
            message
        });
    }

    /// <summary>
    /// Geçersiz istek sonucunu standart formatta döner.
    /// </summary>
    protected IActionResult FailBadRequest(string message)
    {
        return BadRequest(new
        {
            success = false,
            message
        });
    }

    /// <summary>
    /// Bulunamadı sonucunu standart formatta döner.
    /// </summary>
    protected IActionResult FailNotFound(string message)
    {
        return NotFound(new
        {
            success = false,
            message
        });
    }

    /// <summary>
    /// Yasak erişim sonucunu standart formatta döner.
    /// </summary>
    protected IActionResult FailForbidden(string message = "Bu işlem için yetkiniz yok.")
    {
        return StatusCode(StatusCodes.Status403Forbidden, new
        {
            success = false,
            message
        });
    }
}
