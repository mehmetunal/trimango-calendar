namespace TrimangoCalendar.Core.Services;

public class ReservationService : IReservationService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPricingService _pricingService;
    private readonly ILogger<ReservationService> _logger;
    private readonly IMemoryCache _cache;
    
    public ReservationService(
        AppDbContext context,
        IMapper mapper,
        IPricingService pricingService,
        ILogger<ReservationService> logger,
        IMemoryCache cache)
    {
        _context = context;
        _mapper = mapper;
        _pricingService = pricingService;
        _logger = logger;
        _cache = cache;
    }
    
    /// <summary>
    /// CreateAsync methodunu çalıştırır.
    /// </summary>
    public async Task<ReservationDto> CreateAsync(Guid tenantId, CreateReservationDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // 1. Müsaitlik kontrolü (pessimistic lock ile)
            var isAvailable = await IsUnitAvailableWithLockAsync(dto.UnitId, dto.CheckIn, dto.CheckOut);
            if (!isAvailable)
                throw new BusinessException("Seçilen tarihlerde bu birim müsait değil");
            
            // 2. Misafir bul veya oluştur
            var guest = await FindOrCreateGuestAsync(tenantId, dto);
            
            // 3. Fiyat hesapla
            var priceCalculation = await _pricingService.CalculatePriceAsync(new PriceCalculationRequest
            {
                UnitId = dto.UnitId,
                CheckIn = dto.CheckIn,
                CheckOut = dto.CheckOut,
                Adults = dto.Adults,
                Children = dto.Children,
                CurrencyCode = dto.CurrencyCode,
                PromoCode = dto.PromoCode
            });
            
            // 4. Rezervasyon numarası oluştur
            var reservationNumber = await GenerateReservationNumber(tenantId);
            
            // 5. Rezervasyonu oluştur
            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UnitId = dto.UnitId,
                GuestId = guest.Id,
                ReservationNumber = reservationNumber,
                CheckIn = dto.CheckIn,
                CheckOut = dto.CheckOut,
                TotalNights = priceCalculation.TotalNights,
                Adults = dto.Adults,
                Children = dto.Children,
                Infants = dto.Infants,
                Status = ReservationStatus.Pending,
                TotalAmount = priceCalculation.GrandTotal.Amount,
                PaidAmount = 0,
                RemainingAmount = priceCalculation.GrandTotal.Amount,
                CurrencyCode = dto.CurrencyCode,
                TaxAmount = priceCalculation.TaxAmount.Amount,
                ServiceFee = priceCalculation.ServiceFee.Amount,
                DiscountAmount = priceCalculation.Breakdown.PromotionDiscount?.Amount,
                PromoCode = dto.PromoCode,
                SpecialRequests = dto.SpecialRequests,
                Source = dto.Source,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };
            
            _context.Reservations.Add(reservation);
            
            // 6. Rezervasyon geçmişi oluştur
            AddHistoryEntry(reservation, null, ReservationStatus.Pending, "Rezervasyon oluşturuldu");
            
            // 7. Misafir istatistiklerini güncelle
            guest.TotalStays++;
            guest.TotalNights += priceCalculation.TotalNights;
            guest.LastStayAt = dto.CheckIn;
            _context.Guests.Update(guest);
            
            // 8. Promosyon kullanıldıysa sayacı artır
            if (!string.IsNullOrEmpty(dto.PromoCode))
            {
                var promotion = await _context.Promotions
                    .FirstOrDefaultAsync(p => p.Code == dto.PromoCode && p.IsActive);
                if (promotion != null)
                {
                    promotion.UsedCount++;
                    if (promotion.UsedCount >= promotion.MaxUsageCount)
                        promotion.IsActive = false;
                }
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            // Cache temizle
            ClearAvailabilityCache(dto.UnitId, dto.CheckIn, dto.CheckOut);
            
            _logger.LogInformation("Rezervasyon oluşturuldu: {ReservationNumber}", reservationNumber);
            
            // Background job: Onay emaili gönder
            // BackgroundJob.Enqueue<IEmailService>(x => x.SendReservationConfirmationAsync(reservation.Id));
            
            return _mapper.Map<ReservationDto>(reservation);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    /// <summary>
    /// IsUnitAvailableAsync methodunu çalıştırır.
    /// </summary>
    public async Task<bool> IsUnitAvailableAsync(Guid unitId, DateTime checkIn, DateTime checkOut, Guid? excludeReservationId = null)
    {
        // Cache'de var mı kontrol et
        var cacheKey = $"availability_{unitId}_{checkIn:yyyyMMdd}_{checkOut:yyyyMMdd}";
        
        var cached = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            
            var query = _context.Reservations
                .Where(r => r.UnitId == unitId
                    && r.CheckIn < checkOut
                    && r.CheckOut > checkIn
                    && r.Status != ReservationStatus.Cancelled
                    && r.Status != ReservationStatus.NoShow);
            
            if (excludeReservationId.HasValue)
                query = query.Where(r => r.Id != excludeReservationId.Value);
            
            return !await query.AnyAsync();
        });
        
        return cached;
    }
    
    /// <summary>
    /// IsUnitAvailableWithLockAsync methodunu çalıştırır.
    /// </summary>
    private async Task<bool> IsUnitAvailableWithLockAsync(Guid unitId, DateTime checkIn, DateTime checkOut)
    {
        // Pessimistic lock ile müsaitlik kontrolü (race condition'ı önler)
        var conflicting = await _context.Reservations
            .FromSqlRaw(@"
                SELECT * FROM Reservations WITH (UPDLOCK, ROWLOCK)
                WHERE UnitId = {0} 
                AND Status NOT IN (6, 7) -- Cancelled veya NoShow değilse
                AND CheckIn < {2} 
                AND CheckOut > {1}",
                unitId, checkIn, checkOut)
            .AnyAsync();
        
        return !conflicting;
    }
    
    /// <summary>
    /// FindOrCreateGuestAsync methodunu çalıştırır.
    /// </summary>
    private async Task<Guest> FindOrCreateGuestAsync(Guid tenantId, CreateReservationDto dto)
    {
        // Önce mevcut misafiri bul
        var guest = await _context.Guests
            .FirstOrDefaultAsync(g => 
                g.TenantId == tenantId && 
                (g.Email == dto.Email || g.Phone == dto.Phone));
        
        if (guest != null)
        {
            // Misafir bilgilerini güncelle
            guest.FirstName = dto.FirstName;
            guest.LastName = dto.LastName;
            guest.Phone = dto.Phone;
            guest.UpdatedAt = DateTime.UtcNow;
            return guest;
        }
        
        // Yeni misafir oluştur
        guest = new Guest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            TcKimlikNo = dto.TcKimlikNo,
            PassportNumber = dto.PassportNumber,
            Nationality = dto.Nationality ?? "Türkiye",
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Guests.Add(guest);
        return guest;
    }
    
    /// <summary>
    /// CheckInAsync methodunu çalıştırır.
    /// </summary>
    public async Task<ReservationDto> CheckInAsync(Guid reservationId)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Unit)
            .Include(r => r.Guest)
            .FirstOrDefaultAsync(r => r.Id == reservationId);
        
        if (reservation == null)
            throw new NotFoundException("Rezervasyon bulunamadı");
        
        if (reservation.Status != ReservationStatus.Confirmed)
            throw new BusinessException("Sadece onaylanmış rezervasyonlar için check-in yapılabilir");
        
        // Check-in tarihi bugün veya 1 gün öncesi/sonrası olmalı
        var today = DateTime.Today;
        if (Math.Abs((reservation.CheckIn - today).Days) > 1)
            throw new BusinessException("Check-in sadece rezervasyon tarihinde yapılabilir (±1 gün)");
        
        var oldStatus = reservation.Status;
        reservation.Status = ReservationStatus.CheckedIn;
        reservation.ActualCheckIn = DateTime.UtcNow;
        reservation.StatusChangedAt = DateTime.UtcNow;
        reservation.UpdatedAt = DateTime.UtcNow;
        
        AddHistoryEntry(reservation, oldStatus, ReservationStatus.CheckedIn, "Misafir giriş yaptı");
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Check-in yapıldı: {ReservationNumber}", reservation.ReservationNumber);
        
        return _mapper.Map<ReservationDto>(reservation);
    }
    
    /// <summary>
    /// CheckOutAsync methodunu çalıştırır.
    /// </summary>
    public async Task<ReservationDto> CheckOutAsync(Guid reservationId, bool isLate = false)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Unit)
            .Include(r => r.Guest)
            .FirstOrDefaultAsync(r => r.Id == reservationId);
        
        if (reservation == null)
            throw new NotFoundException("Rezervasyon bulunamadı");
        
        if (reservation.Status != ReservationStatus.CheckedIn)
            throw new BusinessException("Sadece check-in yapılmış rezervasyonlar için check-out yapılabilir");
        
        var oldStatus = reservation.Status;
        reservation.Status = ReservationStatus.CheckedOut;
        reservation.ActualCheckOut = DateTime.UtcNow;
        reservation.IsLateCheckout = isLate;
        reservation.StatusChangedAt = DateTime.UtcNow;
        reservation.UpdatedAt = DateTime.UtcNow;
        
        AddHistoryEntry(reservation, oldStatus, ReservationStatus.CheckedOut, 
            isLate ? "Misafir geç çıkış yaptı" : "Misafir çıkış yaptı");
        
        // Ödeme durumunu kontrol et
        if (reservation.RemainingAmount > 0)
        {
            reservation.StatusNote = "Ödeme tamamlanmadı";
        }
        
        await _context.SaveChangesAsync();
        
        // Background job: Değerlendirme emaili gönder
        // BackgroundJob.Schedule<IEmailService>(x => x.SendReviewRequestAsync(reservation.Id), TimeSpan.FromHours(2));
        
        _logger.LogInformation("Check-out yapıldı: {ReservationNumber}", reservation.ReservationNumber);
        
        return _mapper.Map<ReservationDto>(reservation);
    }
    
    /// <summary>
    /// CancelAsync methodunu çalıştırır.
    /// </summary>
    public async Task<bool> CancelAsync(Guid reservationId, string reason)
    {
        var reservation = await _context.Reservations.FindAsync(reservationId);
        if (reservation == null)
            throw new NotFoundException("Rezervasyon bulunamadı");
        
        if (reservation.Status == ReservationStatus.Cancelled)
            throw new BusinessException("Bu rezervasyon zaten iptal edilmiş");
        
        if (reservation.Status == ReservationStatus.CheckedIn)
            throw new BusinessException("Check-in yapılmış rezervasyon iptal edilemez");
        
        var oldStatus = reservation.Status;
        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancelledAt = DateTime.UtcNow;
        reservation.CancellationReason = reason;
        reservation.StatusChangedAt = DateTime.UtcNow;
        reservation.UpdatedAt = DateTime.UtcNow;
        
        // İptal politikasına göre iade tutarı hesapla
        var daysUntilCheckIn = (reservation.CheckIn.Date - DateTime.Today).Days;
        
        if (daysUntilCheckIn >= 7) // 7 günden fazla varsa tam iade
        {
            reservation.RefundAmount = reservation.PaidAmount;
        }
        else if (daysUntilCheckIn >= 3) // 3-7 gün arası %50 iade
        {
            reservation.RefundAmount = reservation.PaidAmount * 0.5m;
        }
        else // 3 günden az iade yok
        {
            reservation.RefundAmount = 0;
        }
        
        AddHistoryEntry(reservation, oldStatus, ReservationStatus.Cancelled, $"İptal: {reason}");
        
        await _context.SaveChangesAsync();
        
        // Cache temizle
        ClearAvailabilityCache(reservation.UnitId, reservation.CheckIn, reservation.CheckOut);
        
        _logger.LogInformation("Rezervasyon iptal edildi: {ReservationNumber}", reservation.ReservationNumber);
        
        return true;
    }
    
    /// <summary>
    /// GenerateReservationNumber methodunu çalıştırır.
    /// </summary>
    private async Task<string> GenerateReservationNumber(Guid tenantId)
    {
        var today = DateTime.Today;
        var prefix = $"R{today:yyyyMMdd}";
        
        // Bugünün kaçıncı rezervasyonu
        var count = await _context.Reservations
            .CountAsync(r => r.TenantId == tenantId && 
                            r.CreatedAt.Date == today);
        
        return $"{prefix}-{(count + 1):D3}";
    }
    
    /// <summary>
    /// AddHistoryEntry methodunu çalıştırır.
    /// </summary>
    private void AddHistoryEntry(Reservation reservation, ReservationStatus? oldStatus, ReservationStatus newStatus, string note)
    {
        reservation.History ??= new List<ReservationHistory>();
        
        reservation.History.Add(new ReservationHistory
        {
            Id = Guid.NewGuid(),
            ReservationId = reservation.Id,
            OldStatus = oldStatus ?? newStatus,
            NewStatus = newStatus,
            Note = note,
            ChangedBy = "System", // TODO: Current user
            ChangedAt = DateTime.UtcNow
        });
    }
    
    /// <summary>
    /// GetAvailabilityAsync methodunu çalıştırır.
    /// </summary>
    public async Task<List<UnitAvailabilityDto>> GetAvailabilityAsync(Guid propertyId, DateTime startDate, DateTime endDate)
    {
        var units = await _context.Units
            .Where(u => u.PropertyId == propertyId && u.IsActive)
            .ToListAsync();
        
        var reservations = await _context.Reservations
            .Where(r => r.Unit.PropertyId == propertyId
                && r.CheckIn < endDate
                && r.CheckOut > startDate
                && r.Status != ReservationStatus.Cancelled
                && r.Status != ReservationStatus.NoShow)
            .ToListAsync();
        
        var result = new List<UnitAvailabilityDto>();
        
        foreach (var unit in units)
        {
            var unitReservations = reservations
                .Where(r => r.UnitId == unit.Id)
                .ToList();
            
            var availability = new UnitAvailabilityDto
            {
                UnitId = unit.Id,
                UnitName = unit.Name,
                UnitNumber = unit.UnitNumber,
                MaxAdults = unit.MaxAdults,
                BasePrice = unit.BasePrice,
                CurrencyCode = unit.CurrencyCode,
                AvailableDates = new List<DateAvailabilityDto>()
            };
            
            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                var isReserved = unitReservations.Any(r => 
                    r.CheckIn <= date && r.CheckOut > date);
                
                var reservation = unitReservations.FirstOrDefault(r => 
                    r.CheckIn <= date && r.CheckOut > date);
                
                availability.AvailableDates.Add(new DateAvailabilityDto
                {
                    Date = date,
                    IsAvailable = !isReserved,
                    ReservationId = reservation?.Id,
                    ReservationNumber = reservation?.ReservationNumber
                });
            }
            
            result.Add(availability);
        }
        
        return result;
    }
    
    /// <summary>
    /// ClearAvailabilityCache methodunu çalıştırır.
    /// </summary>
    private void ClearAvailabilityCache(Guid unitId, DateTime checkIn, DateTime checkOut)
    {
        for (var date = checkIn; date <= checkOut; date = date.AddDays(1))
        {
            _cache.Remove($"availability_{unitId}_{date:yyyyMMdd}_*");
        }
    }
}

