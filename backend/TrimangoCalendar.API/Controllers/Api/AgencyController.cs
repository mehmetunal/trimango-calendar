[ApiController]
[Route("api/[controller]")]
public class AgencyController : ControllerBase
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
    public async Task<IActionResult> GetPropertyAuthorizations(Guid propertyId)
    {
        var authorizations = await _agencyService.GetPropertyAuthorizationsAsync(propertyId);
        return Ok(new { success = true, data = authorizations });
    }
    
    [HttpPost("grant")]
    [Authorize(Roles = "PropertyOwner")]
    public async Task<IActionResult> GrantAuthorization([FromBody] GrantAuthorizationDto dto)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
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
    public async Task<IActionResult> RevokeAuthorization(Guid authId)
    {
        await _agencyService.RevokeAuthorizationAsync(authId);
        return Ok(new { success = true, message = "Yetkilendirme iptal edildi" });
    }
    
    [HttpPost("blocks")]
    [Authorize(Roles = "PropertyOwner")]
    public async Task<IActionResult> BlockDates([FromBody] BlockDatesDto dto)
    {
        try
        {
            dto.CreatedByTenantId = GetCurrentTenantId();
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
    public async Task<IActionResult> UnblockDates(Guid blockId)
    {
        await _calendarService.UnblockDatesAsync(blockId);
        return Ok(new { success = true, message = "Blokaj kaldırıldı" });
    }
    
    [HttpGet("calendar/{propertyId}")]
    [Authorize(Roles = "PropertyOwner")]
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
    public async Task<IActionResult> GetMyProperties()
    {
        var agencyId = GetCurrentAgencyId();
        var properties = await _agencyService.GetAgencyPropertiesAsync(agencyId);
        return Ok(new { success = true, data = properties });
    }
    
    [HttpGet("my-properties/{propertyId}")]
    [Authorize(Roles = "Agency")]
    public async Task<IActionResult> GetMyPropertyDetail(Guid propertyId)
    {
        var agencyId = GetCurrentAgencyId();
        var detail = await _agencyService.GetAgencyPropertyDetailAsync(agencyId, propertyId);
        return Ok(new { success = true, data = detail });
    }
    
    [HttpGet("my-calendar/{propertyId}")]
    [Authorize(Roles = "Agency")]
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
    
    private Guid GetCurrentTenantId()
    {
        return (Guid)HttpContext.Items["TenantId"];
    }
    
    private Guid GetCurrentAgencyId()
    {
        // Agency User'dan AgencyId'yi al
        return Guid.Parse(User.FindFirst("AgencyId")?.Value ?? throw new UnauthorizedAccessException());
    }
}
Harika! Acente yetkilendirme ve takvim sistemi tamamlandı. Bu modül şunları içeriyor:

✅ Acente CRUD ve yönetimi

✅ Mülk sahibi tarafından acentelere yetki verme

✅ Detaylı yetkilendirme seviyeleri (görüntüleme, fiyat, rezervasyon)

✅ Fiyat görüntüleme tipleri (Net, Komisyonlu, Markup)

✅ Kontenjan yönetimi

✅ Takvim bazlı blokaj (bakım, kapalı sezon)

✅ Acente takvimi (müsaitlik, fiyatlar, blokajlar)

✅ Acente fiyat belirleme (markup)

Şimdi sırada MODÜL 6: BİLDİRİM ve RAPORLAMA SİSTEMİ var. Devam edelim mi?

devam edelim
Harika! Bildirim ve raporlama sistemi, projenin vazgeçilmez parçaları. Bu modülü de eksiksiz hazırlayalım.

📦 MODÜL 6: BİLDİRİM & RAPORLAMA SİSTEMİ
