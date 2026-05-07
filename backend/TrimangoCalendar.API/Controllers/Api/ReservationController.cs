[ApiController]
[Route("api/[controller]")]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;
    
    public ReservationController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        try
        {
            var tenantId = GetTenantId();
            var reservation = await _reservationService.CreateAsync(tenantId, dto);
            
            return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, 
                new { success = true, data = reservation });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var reservation = await _reservationService.GetByIdAsync(id);
            return Ok(new { success = true, data = reservation });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }
    
    [HttpGet("number/{reservationNumber}")]
    public async Task<IActionResult> GetByNumber(string reservationNumber)
    {
        try
        {
            var reservation = await _reservationService.GetByNumberAsync(reservationNumber);
            return Ok(new { success = true, data = reservation });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetByTenant([FromQuery] ReservationFilterDto filter)
    {
        var tenantId = GetTenantId();
        var result = await _reservationService.GetByTenantAsync(tenantId, filter);
        return Ok(new { success = true, data = result });
    }
    
    [HttpGet("availability/{propertyId}")]
    public async Task<IActionResult> GetAvailability(
        Guid propertyId, 
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        if (startDate >= endDate)
            return BadRequest(new { success = false, message = "Başlangıç tarihi bitiş tarihinden önce olmalıdır" });
        
        var availability = await _reservationService.GetAvailabilityAsync(propertyId, startDate, endDate);
        return Ok(new { success = true, data = availability });
    }
    
    [HttpGet("check-availability/{unitId}")]
    public async Task<IActionResult> CheckAvailability(
        Guid unitId,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut)
    {
        var isAvailable = await _reservationService.IsUnitAvailableAsync(unitId, checkIn, checkOut);
        return Ok(new { success = true, isAvailable });
    }
    
    [HttpPost("{id}/check-in")]
    [Authorize]
    public async Task<IActionResult> CheckIn(Guid id)
    {
        try
        {
            var reservation = await _reservationService.CheckInAsync(id);
            return Ok(new { success = true, data = reservation });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }
    
    [HttpPost("{id}/check-out")]
    [Authorize]
    public async Task<IActionResult> CheckOut(Guid id, [FromQuery] bool isLate = false)
    {
        try
        {
            var reservation = await _reservationService.CheckOutAsync(id, isLate);
            return Ok(new { success = true, data = reservation });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] string reason)
    {
        try
        {
            await _reservationService.CancelAsync(id, reason);
            return Ok(new { success = true, message = "Rezervasyon iptal edildi" });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    
    [HttpPut("status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateReservationStatusDto dto)
    {
        try
        {
            var reservation = await _reservationService.UpdateStatusAsync(dto);
            return Ok(new { success = true, data = reservation });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var tenantId = GetTenantId();
        var stats = await _reservationService.GetStatsAsync(tenantId, startDate, endDate);
        return Ok(new { success = true, data = stats });
    }
    
    [HttpGet("calendar")]
    public async Task<IActionResult> GetCalendar(
        [FromQuery] Guid? propertyId,
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? end)
    {
        // Takvim görünümü için rezervasyonları getir
        var tenantId = GetTenantId();
        var startDate = start ?? DateTime.Today.AddDays(-30);
        var endDate = end ?? DateTime.Today.AddDays(60);
        
        var filter = new ReservationFilterDto
        {
            CheckInFrom = startDate,
            CheckOutTo = endDate,
            PageSize = 1000
        };
        
        if (propertyId.HasValue)
            filter.PropertyId = propertyId;
        
        var reservations = await _reservationService.GetByTenantAsync(tenantId, filter);
        
        // FullCalendar formatına dönüştür
        var events = reservations.Items.Select(r => new
        {
            id = r.Id,
            title = $"{r.GuestName} - {r.UnitName}",
            start = r.CheckIn.ToString("yyyy-MM-dd"),
            end = r.CheckOut.ToString("yyyy-MM-dd"),
            backgroundColor = GetStatusColor(r.Status),
            extendedProps = new
            {
                reservationNumber = r.ReservationNumber,
                status = r.Status,
                guestEmail = r.GuestEmail,
                guestPhone = r.GuestPhone
            }
        });
        
        return Ok(events);
    }
    
    private string GetStatusColor(string status)
    {
        return status switch
        {
            "Confirmed" => "#4CAF50",
            "CheckedIn" => "#2196F3",
            "CheckedOut" => "#9E9E9E",
            "Cancelled" => "#F44336",
            "NoShow" => "#FF9800",
            "Pending" => "#FFC107",
            _ => "#9E9E9E"
        };
    }
    
    private Guid GetTenantId()
    {
        if (HttpContext.Items["TenantId"] is Guid tenantId)
            return tenantId;
            
        // Admin panel için default tenant
        return Guid.Empty;
    }
}
Rezervasyon modülü tamamlandı! Bu modül şunları içeriyor:

✅ Tam rezervasyon CRUD işlemleri

✅ Pessimistic lock ile race condition önleme

✅ Müsaitlik kontrolü ve takvimi

✅ Check-in/Check-out yönetimi

✅ İptal ve iade politikası

✅ Misafir yönetimi

✅ Rezervasyon geçmişi takibi

✅ İstatistikler ve dashboard verileri

Şimdi sırada MODÜL 5: ÖDEME SİSTEMİ var. İyzico ve PayTR entegrasyonlarını içerecek. Devam edelim mi?

devam edelim ama ödemeler bizim üzerimizden olmucak, bizim sadece takvim kapatma açma bana ait bir villayı oteli aşka acentanın görebilmesi için işlem yapabilmesi için yetki verebilmesi gerek bunlarda olacak
Anladım! Ödeme sizin üzerinizden olmayacak, sadece Channel Manager gibi bir yapı kuruyoruz. Yani:

Mülk sahibi sisteme kaydoluyor

Mülkünü, odalarını tanımlıyor

Acentelere yetki veriyor (hangi mülkleri görebilecekleri, hangi işlemleri yapabilecekleri)

Acenteler kendilerine verilen mülkler için takvim görüp, fiyat girip, rezervasyon yapabiliyor

Mülk sahibi tüm hareketleri görebiliyor

Bu harika bir iş modeli! Hemen buna göre sistemi kuralım.

📦 MODÜL 5: ACENTE YETKİLENDİRME & TAKVİM YÖNETİM SİSTEMİ
