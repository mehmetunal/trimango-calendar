public class CalendarService : ICalendarService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    
    public CalendarService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<CalendarBlockDto> BlockDatesAsync(BlockDatesDto dto)
    {
        // Tarih çakışması kontrolü
        var hasConflict = await CheckBlockConflict(dto.UnitId, dto.StartDate, dto.EndDate);
        if (hasConflict)
            throw new BusinessException("Bu tarih aralığında zaten bir blokaj var");
        
        var block = new CalendarBlock
        {
            Id = Guid.NewGuid(),
            UnitId = dto.UnitId,
            PropertyId = dto.PropertyId,
            Type = dto.Type,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Reason = dto.Reason,
            Notes = dto.Notes,
            CreatedByTenantId = dto.CreatedByTenantId,
            CreatedByAgencyId = dto.CreatedByAgencyId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.CalendarBlocks.Add(block);
        await _context.SaveChangesAsync();
        
        return _mapper.Map<CalendarBlockDto>(block);
    }
    
    public async Task<AgencyCalendarDto> GetAgencyCalendarAsync(Guid agencyId, Guid propertyId, DateTime start, DateTime end)
    {
        // Yetki kontrolü
        var auth = await _context.AgencyAuthorizations
            .FirstOrDefaultAsync(a => 
                a.AgencyId == agencyId && 
                a.PropertyId == propertyId && 
                a.IsActive);
        
        if (auth == null)
            throw new BusinessException("Bu mülk için yetkiniz yok");
        
        // Hangi birimleri görebilir?
        var allowedUnitIds = auth.AllowedUnitIds == "*"
            ? await _context.Units.Where(u => u.PropertyId == propertyId && u.IsActive).Select(u => u.Id).ToListAsync()
            : JsonSerializer.Deserialize<List<Guid>>(auth.AllowedUnitIds);
        
        var units = await _context.Units
            .Where(u => allowedUnitIds.Contains(u.Id))
            .ToListAsync();
        
        // Blokajları getir
        var blocks = await _context.CalendarBlocks
            .Where(b => allowedUnitIds.Contains(b.UnitId) && 
                       b.StartDate < end && b.EndDate > start &&
                       b.IsActive)
            .ToListAsync();
        
        // Rezervasyonları getir
        var reservations = await _context.Reservations
            .Where(r => allowedUnitIds.Contains(r.UnitId) &&
                       r.CheckIn < end && r.CheckOut > start &&
                       r.Status != ReservationStatus.Cancelled &&
                       r.Status != ReservationStatus.NoShow)
            .ToListAsync();
        
        // Fiyatları getir (eğer görme yetkisi varsa)
        List<CalendarPrice> prices = null;
        if (auth.CanViewPrices)
        {
            prices = await _context.CalendarPrices
                .Where(p => allowedUnitIds.Contains(p.UnitId) && 
                           p.Date >= start && p.Date <= end)
                .ToListAsync();
        }
        
        // Takvim verisini oluştur
        var calendar = new AgencyCalendarDto
        {
            PropertyId = propertyId,
            PropertyName = (await _context.Properties.FindAsync(propertyId))?.Name,
            StartDate = start,
            EndDate = end,
            CanViewPrices = auth.CanViewPrices,
            CanSetPrices = auth.CanSetPrices,
            CanCreateReservation = auth.CanCreateReservation,
            PriceDisplay = auth.PriceDisplay.ToString(),
            CommissionRate = auth.CustomCommissionRate ?? 
                (await _context.Agencies.FindAsync(agencyId))?.DefaultCommissionRate ?? 10,
            DefaultMarkupRate = auth.DefaultMarkupRate,
            Units = new List<UnitCalendarDto>()
        };
        
        foreach (var unit in units)
        {
            var unitBlock = blocks.Where(b => b.UnitId == unit.Id).ToList();
            var unitReservations = reservations.Where(r => r.UnitId == unit.Id).ToList();
            
            var dailyData = new List<DailyCalendarDto>();
            
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                var block = unitBlock.FirstOrDefault(b => date >= b.StartDate && date <= b.EndDate);
                var reservation = unitReservations.FirstOrDefault(r => date >= r.CheckIn && date < r.CheckOut);
                
                var dailyPrice = prices?.FirstOrDefault(p => p.UnitId == unit.Id && p.Date == date);
                
                var status = CalendarDayStatus.Available;
                if (block != null)
                    status = CalendarDayStatus.Blocked;
                else if (reservation != null)
                    status = CalendarDayStatus.Reserved;
                
                dailyData.Add(new DailyCalendarDto
                {
                    Date = date,
                    Status = status,
                    StatusDescription = GetStatusDescription(status, block, reservation),
                    ReservationNumber = reservation?.ReservationNumber,
                    GuestName = reservation != null ? 
                        $"{reservation.Guest?.FirstName} {reservation.Guest?.LastName}" : null,
                    BasePrice = dailyPrice?.Price ?? unit.BasePrice,
                    CurrencyCode = unit.CurrencyCode,
                    AgencyPrice = CalculateAgencyPrice(
                        dailyPrice?.Price ?? unit.BasePrice, 
                        auth),
                    BlockReason = block?.Reason
                });
            }
            
            calendar.Units.Add(new UnitCalendarDto
            {
                UnitId = unit.Id,
                UnitName = unit.Name,
                UnitNumber = unit.UnitNumber,
                DailyData = dailyData
            });
        }
        
        return calendar;
    }
    
    private decimal CalculateAgencyPrice(decimal basePrice, AgencyAuthorization auth)
    {
        return auth.PriceDisplay switch
        {
            PriceDisplayType.Net => basePrice,
            PriceDisplayType.Commission => basePrice * (1 + (auth.CustomCommissionRate ?? 10) / 100),
            PriceDisplayType.Markup => basePrice * (1 + (auth.DefaultMarkupRate ?? 10) / 100),
            _ => basePrice
        };
    }
    
    private string GetStatusDescription(CalendarDayStatus status, CalendarBlock block, Reservation reservation)
    {
        return status switch
        {
            CalendarDayStatus.Available => "Müsait",
            CalendarDayStatus.Blocked => $"Kapalı: {block?.Reason ?? "Bilinmiyor"}",
            CalendarDayStatus.Reserved => $"Rezerve: {reservation?.Guest?.FirstName} {reservation?.Guest?.LastName}",
            _ => "Bilinmiyor"
        };
    }
}

