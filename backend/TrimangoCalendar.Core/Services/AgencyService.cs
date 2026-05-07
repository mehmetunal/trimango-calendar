public class AgencyService : IAgencyService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    
    public AgencyService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<AuthorizationDto> GrantAuthorizationAsync(Guid ownerTenantId, GrantAuthorizationDto dto)
    {
        // Mülk sahibinin bu mülke yetkisi var mı?
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == dto.PropertyId && p.TenantId == ownerTenantId);
        
        if (property == null)
            throw new BusinessException("Bu mülk için yetkilendirme yapma hakkınız yok");
        
        // Acente var mı?
        var agency = await _context.Agencies
            .FirstOrDefaultAsync(a => a.Id == dto.AgencyId && a.IsActive);
        
        if (agency == null)
            throw new NotFoundException("Acente bulunamadı");
        
        // Zaten yetki var mı?
        var existing = await _context.AgencyAuthorizations
            .FirstOrDefaultAsync(a => a.AgencyId == dto.AgencyId && a.PropertyId == dto.PropertyId);
        
        if (existing != null)
            throw new BusinessException("Bu acente için zaten yetkilendirme mevcut");
        
        var authorization = new AgencyAuthorization
        {
            Id = Guid.NewGuid(),
            AgencyId = dto.AgencyId,
            PropertyId = dto.PropertyId,
            GrantedByTenantId = ownerTenantId,
            Level = dto.Level,
            AllowedUnitIds = dto.AllowedUnitIds != null 
                ? JsonSerializer.Serialize(dto.AllowedUnitIds) 
                : "*", // Tümü
            CanViewPrices = dto.CanViewPrices,
            CanSetPrices = dto.CanSetPrices,
            CanCreateReservation = dto.CanCreateReservation,
            CanModifyReservation = dto.CanModifyReservation,
            CanCancelReservation = dto.CanCancelReservation,
            PriceDisplay = dto.PriceDisplay,
            CustomCommissionRate = dto.CustomCommissionRate,
            MaxMarkupRate = dto.MaxMarkupRate,
            DefaultMarkupRate = dto.DefaultMarkupRate,
            HasAllotment = dto.HasAllotment,
            TotalAllotment = dto.TotalAllotment,
            UsedAllotment = 0,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            IsActive = true,
            Notes = dto.Notes,
            GrantedAt = DateTime.UtcNow,
            GrantedBy = "System" // TODO: Current user
        };
        
        _context.AgencyAuthorizations.Add(authorization);
        await _context.SaveChangesAsync();
        
        // TODO: Acenteye email gönder - "Yeni mülk yetkilendirmesi"
        
        return _mapper.Map<AuthorizationDto>(authorization);
    }
    
    public async Task RevokeAuthorizationAsync(Guid authId)
    {
        var auth = await _context.AgencyAuthorizations.FindAsync(authId);
        if (auth == null)
            throw new NotFoundException("Yetkilendirme bulunamadı");
        
        auth.IsActive = false;
        auth.RevokedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
    }
    
    public async Task<List<AuthorizedPropertyDto>> GetAgencyPropertiesAsync(Guid agencyId)
    {
        var authorizations = await _context.AgencyAuthorizations
            .Include(a => a.Property)
                .ThenInclude(p => p.Units)
            .Where(a => a.AgencyId == agencyId && a.IsActive)
            .ToListAsync();
        
        return authorizations.Select(a =>
        {
            var units = a.AllowedUnitIds == "*" 
                ? a.Property.Units.ToList() 
                : a.Property.Units.Where(u => 
                    JsonSerializer.Deserialize<List<Guid>>(a.AllowedUnitIds)
                        .Contains(u.Id)).ToList();
            
            return new AuthorizedPropertyDto
            {
                AuthorizationId = a.Id,
                PropertyId = a.PropertyId,
                PropertyName = a.Property.Name,
                PropertyType = a.Property.Type.ToString(),
                City = a.Property.City,
                TotalUnits = units.Count,
                ActiveReservations = 0, // TODO: Hesapla
                CanCreateReservation = a.CanCreateReservation,
                CanSetPrices = a.CanSetPrices,
                PriceDisplay = a.PriceDisplay.ToString(),
                CommissionRate = a.CustomCommissionRate ?? a.Agency.DefaultCommissionRate,
                DefaultMarkupRate = a.DefaultMarkupRate,
                RemainingAllotment = a.TotalAllotment.HasValue 
                    ? a.TotalAllotment.Value - (a.UsedAllotment ?? 0) 
                    : null,
                IsActive = a.IsActive
            };
        }).ToList();
    }
    
    public async Task<AuthorizedPropertyDetailDto> GetAgencyPropertyDetailAsync(Guid agencyId, Guid propertyId)
    {
        var auth = await _context.AgencyAuthorizations
            .Include(a => a.Property)
                .ThenInclude(p => p.Units)
            .Include(a => a.Agency)
            .FirstOrDefaultAsync(a => 
                a.AgencyId == agencyId && 
                a.PropertyId == propertyId && 
                a.IsActive);
        
        if (auth == null)
            throw new BusinessException("Bu mülk için yetkiniz yok");
        
        // Hangi birimleri görebilir?
        var allowedUnits = auth.AllowedUnitIds == "*"
            ? auth.Property.Units.Where(u => u.IsActive).ToList()
            : auth.Property.Units.Where(u => 
                JsonSerializer.Deserialize<List<Guid>>(auth.AllowedUnitIds)
                    .Contains(u.Id) && u.IsActive).ToList();
        
        return new AuthorizedPropertyDetailDto
        {
            AuthorizationId = auth.Id,
            PropertyId = auth.Property.Id,
            PropertyName = auth.Property.Name,
            PropertyDescription = auth.Property.Description,
            PropertyType = auth.Property.Type.ToString(),
            Address = auth.Property.Address,
            City = auth.Property.City,
            CheckInTime = auth.Property.CheckInTime.ToString(@"hh\:mm"),
            CheckOutTime = auth.Property.CheckOutTime.ToString(@"hh\:mm"),
            
            // Yetki detayları
            AuthorizationLevel = auth.Level.ToString(),
            CanViewPrices = auth.CanViewPrices,
            CanSetPrices = auth.CanSetPrices,
            CanCreateReservation = auth.CanCreateReservation,
            CanModifyReservation = auth.CanModifyReservation,
            CanCancelReservation = auth.CanCancelReservation,
            
            // Fiyat politikası
            PriceDisplay = auth.PriceDisplay.ToString(),
            CommissionRate = auth.CustomCommissionRate ?? auth.Agency.DefaultCommissionRate,
            DefaultMarkupRate = auth.DefaultMarkupRate,
            MaxMarkupRate = auth.MaxMarkupRate,
            
            // Kontenjan
            HasAllotment = auth.HasAllotment,
            TotalAllotment = auth.TotalAllotment,
            UsedAllotment = auth.UsedAllotment ?? 0,
            
            // Birimler
            Units = allowedUnits.Select(u => new AuthorizedUnitDto
            {
                UnitId = u.Id,
                UnitName = u.Name,
                UnitNumber = u.UnitNumber,
                MaxAdults = u.MaxAdults,
                MaxChildren = u.MaxChildren,
                BasePrice = auth.CanViewPrices ? u.BasePrice : null,
                CurrencyCode = u.CurrencyCode,
                IsActive = u.IsActive
            }).ToList(),
            
            ValidFrom = auth.ValidFrom,
            ValidTo = auth.ValidTo
        };
    }
    
    public async Task<bool> CheckAllotmentAvailabilityAsync(Guid authId)
    {
        var auth = await _context.AgencyAuthorizations.FindAsync(authId);
        if (auth == null || !auth.HasAllotment)
            return true; // Kontenjan yoksa sınırsız
        
        return (auth.UsedAllotment ?? 0) < auth.TotalAllotment;
    }
}

