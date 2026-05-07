public class BookingEngineService : IBookingEngineService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPricingService _pricingService;
    private readonly IReservationService _reservationService;
    private readonly INotificationService _notificationService;
    private readonly IMemoryCache _cache;
    
    public BookingEngineService(
        AppDbContext context,
        IMapper mapper,
        IPricingService pricingService,
        IReservationService reservationService,
        INotificationService notificationService,
        IMemoryCache cache)
    {
        _context = context;
        _mapper = mapper;
        _pricingService = pricingService;
        _reservationService = reservationService;
        _notificationService = notificationService;
        _cache = cache;
    }
    
    public async Task<AvailabilityResultDto> CheckAvailabilityAsync(string widgetKey, AvailabilitySearchDto search)
    {
        var widget = await GetActiveWidgetAsync(widgetKey);
        if (widget == null)
            throw new NotFoundException("Widget bulunamadı veya aktif değil");
        
        // Tarih validasyonu
        if (search.CheckIn < DateTime.Today.AddDays(widget.MinAdvanceDays))
            throw new BusinessException($"En az {widget.MinAdvanceDays} gün önceden rezervasyon yapılabilir");
        
        if (search.CheckIn > DateTime.Today.AddDays(widget.MaxAdvanceDays))
            throw new BusinessException($"En fazla {widget.MaxAdvanceDays} gün ileri rezervasyon yapılabilir");
        
        // Müsait birimleri bul
        var units = await _context.Units
            .Where(u => u.PropertyId == widget.PropertyId && u.IsActive)
            .ToListAsync();
        
        var availableUnits = new List<AvailableUnitDto>();
        
        foreach (var unit in units)
        {
            // Kapasite kontrolü
            if (unit.MaxAdults < search.Adults || 
                unit.MaxChildren < (search.Children ?? 0))
                continue;
            
            // Müsaitlik kontrolü
            var isAvailable = await _reservationService.IsUnitAvailableAsync(
                unit.Id, search.CheckIn, search.CheckOut);
            
            if (!isAvailable) continue;
            
            // Fiyat hesapla
            var priceCalculation = await _pricingService.CalculatePriceAsync(new PriceCalculationRequest
            {
                UnitId = unit.Id,
                CheckIn = search.CheckIn,
                CheckOut = search.CheckOut,
                Adults = search.Adults,
                Children = search.Children ?? 0,
                CurrencyCode = search.CurrencyCode ?? "TRY"
            });
            
            availableUnits.Add(new AvailableUnitDto
            {
                UnitId = unit.Id,
                UnitName = unit.Name,
                UnitNumber = unit.UnitNumber,
                Description = unit.Description,
                MaxAdults = unit.MaxAdults,
                MaxChildren = unit.MaxChildren,
                Size = unit.Size,
                View = unit.View,
                ImageUrl = null, // TODO: İlk resmi getir
                Amenities = !string.IsNullOrEmpty(unit.RoomAmenities) 
                    ? JsonSerializer.Deserialize<List<string>>(unit.RoomAmenities) 
                    : new List<string>(),
                
                // Fiyat bilgileri
                TotalPrice = priceCalculation.GrandTotal.Amount,
                CurrencyCode = priceCalculation.GrandTotal.CurrencyCode,
                FormattedTotalPrice = priceCalculation.GrandTotal.ToFormattedString(),
                AverageNightlyPrice = priceCalculation.AverageNightlyPrice.Amount,
                TotalNights = priceCalculation.TotalNights,
                
                // Fiyat kırılımı
                BasePrice = priceCalculation.Breakdown.BasePrice.Amount,
                TaxAmount = priceCalculation.TaxAmount.Amount,
                ServiceFee = priceCalculation.ServiceFee.Amount,
                DiscountAmount = priceCalculation.Breakdown.PromotionDiscount?.Amount,
                
                // Fiyat kırılımı (gösterilecek)
                PriceBreakdown = new List<PriceLineItem>
                {
                    new() { Label = $"{priceCalculation.TotalNights} gece konaklama", Amount = priceCalculation.Breakdown.BasePrice.Amount },
                    new() { Label = "Vergiler (%12)", Amount = priceCalculation.TaxAmount.Amount },
                    new() { Label = "Servis ücreti (%3)", Amount = priceCalculation.ServiceFee.Amount }
                }
            });
        }
        
        return new AvailabilityResultDto
        {
            PropertyId = widget.PropertyId,
            PropertyName = widget.Property.Name,
            CheckIn = search.CheckIn,
            CheckOut = search.CheckOut,
            Adults = search.Adults,
            Children = search.Children ?? 0,
            TotalNights = (search.CheckOut - search.CheckIn).Days,
            AvailableUnits = availableUnits.OrderBy(u => u.TotalPrice).ToList(),
            CurrencyCode = search.CurrencyCode ?? "TRY"
        };
    }
    
    public async Task<ReservationDto> CreateBookingAsync(string widgetKey, CreateBookingDto dto)
    {
        var widget = await GetActiveWidgetAsync(widgetKey);
        if (widget == null)
            throw new NotFoundException("Widget bulunamadı veya aktif değil");
        
        // Müsaitlik son kontrolü
        var isAvailable = await _reservationService.IsUnitAvailableAsync(
            dto.UnitId, dto.CheckIn, dto.CheckOut);
        
        if (!isAvailable)
            throw new BusinessException("Üzgünüz, seçtiğiniz birim bu tarihler için müsait değil");
        
        // Rezervasyonu oluştur
        var createReservationDto = new CreateReservationDto
        {
            UnitId = dto.UnitId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            CheckIn = dto.CheckIn,
            CheckOut = dto.CheckOut,
            Adults = dto.Adults,
            Children = dto.Children,
            Infants = dto.Infants,
            CurrencyCode = dto.CurrencyCode ?? "TRY",
            SpecialRequests = dto.SpecialRequests,
            Source = ReservationSource.Website,
            TcKimlikNo = dto.TcKimlikNo,
            PassportNumber = dto.PassportNumber,
            Nationality = dto.Nationality
        };
        
        var reservation = await _reservationService.CreateAsync(widget.Property.TenantId, createReservationDto);
        
        // Misafire onay emaili gönder
        await _notificationService.SendAsync(
            widget.Property.TenantId,
            NotificationType.NewReservation,
            new Dictionary<string, string>
            {
                { "ReservationNumber", reservation.ReservationNumber },
                { "GuestName", $"{dto.FirstName} {dto.LastName}" },
                { "PropertyName", widget.Property.Name },
                { "UnitName", reservation.UnitName },
                { "CheckInDate", dto.CheckIn.ToString("dd.MM.yyyy") },
                { "CheckOutDate", dto.CheckOut.ToString("dd.MM.yyyy") },
                { "TotalAmount", reservation.TotalAmount.ToString("N2") },
                { "CurrencyCode", reservation.CurrencyCode }
            },
            reservation.Id,
            "Reservation"
        );
        
        return reservation;
    }
    
    public async Task<ReservationDto> GetBookingAsync(string widgetKey, string reservationNumber, string email)
    {
        var widget = await GetActiveWidgetAsync(widgetKey);
        if (widget == null)
            throw new NotFoundException("Widget bulunamadı");
        
        var reservation = await _context.Reservations
            .Include(r => r.Unit)
                .ThenInclude(u => u.Property)
            .Include(r => r.Guest)
            .FirstOrDefaultAsync(r => 
                r.ReservationNumber == reservationNumber && 
                r.Guest.Email == email &&
                r.Unit.PropertyId == widget.PropertyId);
        
        if (reservation == null)
            throw new NotFoundException("Rezervasyon bulunamadı");
        
        return _mapper.Map<ReservationDto>(reservation);
    }
    
    public async Task<bool> CancelBookingAsync(string widgetKey, string reservationNumber, string email, string reason)
    {
        var reservationDto = await GetBookingAsync(widgetKey, reservationNumber, email);
        
        // İptal politikası kontrolü
        var hoursUntilCheckIn = (reservationDto.CheckIn - DateTime.Now).TotalHours;
        
        if (hoursUntilCheckIn < 24)
            throw new BusinessException("Check-in'e 24 saatten az kaldığı için online iptal yapılamaz. Lütfen tesis ile iletişime geçin.");
        
        await _reservationService.CancelAsync(reservationDto.Id, reason);
        
        return true;
    }
    
    public async Task<string> GetWidgetEmbedCode(string widgetKey)
    {
        var widget = await GetActiveWidgetAsync(widgetKey);
        if (widget == null)
            throw new NotFoundException("Widget bulunamadı");
        
        var baseUrl = "https://yourdomain.com"; // Config'den al
        
        return $@"
<!-- HotelPlatform Booking Widget -->
<div id='hp-booking-widget' data-widget-key='{widgetKey}'></div>
<script>
(function(w,d,s,o,f,js,fjs){{
    w['HotelPlatformWidget']=o;
    w[o]=w[o]||function(){{(w[o].q=w[o].q||[]).push(arguments)}};
    js=d.createElement(s),fjs=d.getElementsByTagName(s)[0];
    js.id=o;js.src=f;js.async=1;fjs.parentNode.insertBefore(js,fjs);
}}(window,document,'script','hpw','{baseUrl}/widget.js'));
hpw('init', '{{widgetKey}}');
</script>
<!-- End HotelPlatform Booking Widget -->";
    }
    
    private async Task<BookingWidget> GetActiveWidgetAsync(string widgetKey)
    {
        return await _cache.GetOrCreateAsync($"widget_{widgetKey}", async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            
            return await _context.BookingWidgets
                .Include(w => w.Property)
                    .ThenInclude(p => p.Images)
                .Include(w => w.Integrations)
                .FirstOrDefaultAsync(w => w.WidgetKey == widgetKey && w.IsActive);
        });
    }
}
