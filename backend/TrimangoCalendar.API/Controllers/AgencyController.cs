using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrimangoCalendar.API.Contracts;

[ApiController]
[Route("api/[controller]")]
public class AgencyController : BaseController
{
    private readonly IAgencyService _agencyService;
    private readonly ICalendarService _calendarService;
    private readonly IReservationService _reservationService;

    public AgencyController(
        IAgencyService agencyService,
        ICalendarService calendarService,
        IReservationService reservationService)
    {
        _agencyService = agencyService;
        _calendarService = calendarService;
        _reservationService = reservationService;
    }

    // ========== MÜLK SAHİBİ İŞLEMLERİ ==========

    [HttpGet("authorizations/{propertyId}")]
    [Authorize(Roles = "PropertyOwner")]
    [ProducesResponseType(typeof(ApiResponseDto<List<AuthorizationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    /// <summary>
    /// GetPropertyAuthorizations methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> GetPropertyAuthorizations(Guid propertyId)
    {
        var authorizations = await _agencyService.GetPropertyAuthorizationsAsync(propertyId);
        return Ok(new { success = true, data = authorizations });
    }

    [HttpPost("grant")]
    [Authorize(Roles = "PropertyOwner")]
    [ProducesResponseType(typeof(ApiResponseDto<AuthorizationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    /// <summary>
    /// GrantAuthorization methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> GrantAuthorization([FromBody] GrantAuthorizationDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var authorization = await _agencyService.GrantAuthorizationAsync(tenantId, dto);
            return Ok(new { success = true, data = authorization, message = "Yetkilendirme başarıyla verildi" });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("authorizations/{authId}")]
    [Authorize(Roles = "PropertyOwner")]
    [ProducesResponseType(typeof(ApiResponseDto<AuthorizationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    /// <summary>
    /// UpdateAuthorization methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> UpdateAuthorization(Guid authId, [FromBody] UpdateAuthorizationDto dto)
    {
        try
        {
            var authorization = await _agencyService.UpdateAuthorizationAsync(authId, dto);
            return Ok(new { success = true, data = authorization });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpDelete("authorizations/{authId}")]
    [Authorize(Roles = "PropertyOwner")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    /// <summary>
    /// RevokeAuthorization methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> RevokeAuthorization(Guid authId)
    {
        await _agencyService.RevokeAuthorizationAsync(authId);
        return Ok(new { success = true, message = "Yetkilendirme iptal edildi" });
    }

    [HttpPost("blocks")]
    [Authorize(Roles = "PropertyOwner")]
    [ProducesResponseType(typeof(ApiResponseDto<CalendarBlockDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    /// <summary>
    /// BlockDates methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> BlockDates([FromBody] BlockDatesDto dto)
    {
        try
        {
            dto.CreatedByTenantId = GetTenantId();
            var block = await _calendarService.BlockDatesAsync(dto);
            return Ok(new { success = true, data = block, message = "Tarihler bloke edildi" });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpDelete("blocks/{blockId}")]
    [Authorize(Roles = "PropertyOwner")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    /// <summary>
    /// UnblockDates methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> UnblockDates(Guid blockId)
    {
        await _calendarService.UnblockDatesAsync(blockId);
        return Ok(new { success = true, message = "Blokaj kaldırıldı" });
    }

    [HttpGet("calendar/{propertyId}")]
    [Authorize(Roles = "PropertyOwner")]
    [ProducesResponseType(typeof(ApiResponseDto<OwnerCalendarDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOwnerCalendar(
        Guid propertyId,
        [FromQuery] DateTime start,
        [FromQuery] DateTime end)
    {
        var calendar = await _calendarService.GetOwnerCalendarAsync(propertyId, start, end);
        return Ok(new { success = true, data = calendar });
    }

    // ========== ACENTE İŞLEMLERİ ==========

    [HttpGet("my-properties")]
    [Authorize(Roles = "Agency")]
    [ProducesResponseType(typeof(ApiResponseDto<List<AuthorizedPropertyDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    /// <summary>
    /// GetMyProperties methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> GetMyProperties()
    {
        var agencyId = GetCurrentAgencyId();
        var properties = await _agencyService.GetAgencyPropertiesAsync(agencyId);
        return Ok(new { success = true, data = properties });
    }

    [HttpGet("my-properties/{propertyId}")]
    [Authorize(Roles = "Agency")]
    [ProducesResponseType(typeof(ApiResponseDto<AuthorizedPropertyDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    /// <summary>
    /// GetMyPropertyDetail methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> GetMyPropertyDetail(Guid propertyId)
    {
        var agencyId = GetCurrentAgencyId();
        var detail = await _agencyService.GetAgencyPropertyDetailAsync(agencyId, propertyId);
        return Ok(new { success = true, data = detail });
    }

    [HttpGet("my-calendar/{propertyId}")]
    [Authorize(Roles = "Agency")]
    [ProducesResponseType(typeof(ApiResponseDto<AgencyCalendarDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyCalendar(
        Guid propertyId,
        [FromQuery] DateTime start,
        [FromQuery] DateTime end)
    {
        var agencyId = GetCurrentAgencyId();
        var calendar = await _calendarService.GetAgencyCalendarAsync(agencyId, propertyId, start, end);
        return Ok(new { success = true, data = calendar });
    }

    [HttpPost("my-reservations")]
    [Authorize(Roles = "Agency")]
    [ProducesResponseType(typeof(ApiResponseDto<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    /// <summary>
    /// CreateReservation methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> CreateReservation([FromBody] CreateAgencyReservationDto dto)
    {
        var agencyId = GetCurrentAgencyId();

        // Yetki kontrolü
        var canBook = await _calendarService.CanAgencyBookAsync(
            agencyId, dto.UnitId, dto.CheckIn, dto.CheckOut);

        if (!canBook)
            return BadRequest(new { success = false, message = "Bu tarihler için rezervasyon yetkiniz yok" });

        // Kontenjan kontrolü
        var hasAllotment = await _agencyService.CheckAllotmentAvailabilityAsync(dto.AuthorizationId);
        if (!hasAllotment)
            return BadRequest(new { success = false, message = "Kontenjanınız dolmuş" });

        try
        {
            var reservation = await _reservationService.CreateAgencyReservationAsync(agencyId, dto);
            return Ok(new { success = true, data = reservation });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("my-prices")]
    [Authorize(Roles = "Agency")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    /// <summary>
    /// SetDailyPrice methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> SetDailyPrice([FromBody] SetDailyPriceDto dto)
    {
        // Acentenin fiyat belirleme yetkisi var mı?
        var agencyId = GetCurrentAgencyId();
        var hasPermission = await _agencyService.CanSetPriceAsync(agencyId, dto.UnitId);

        if (!hasPermission)
            return Forbid();

        dto.SetByAgencyId = agencyId;
        dto.Source = PriceSource.AgencyPrice;

        await _calendarService.SetDailyPriceAsync(dto);
        return Ok(new { success = true, message = "Fiyat güncellendi" });
    }
    /// <summary>
    /// GetCurrentAgencyId methodunu çalıştırır.
    /// </summary>
    private Guid GetCurrentAgencyId()
    {
        // Agency User'dan AgencyId'yi al
        return Guid.Parse(User.FindFirst("AgencyId")?.Value ?? throw new UnauthorizedAccessException());
    }
}
