namespace TrimangoCalendar.Core.Services;

public class UnitService : IUnitService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    
    public UnitService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    /// <summary>
    /// CreateAsync methodunu çalıştırır.
    /// </summary>
    public async Task<UnitDto> CreateAsync(Guid propertyId, CreateUnitDto dto)
    {
        // Property kontrol
        var property = await _context.Properties.FindAsync(propertyId);
        if (property == null)
            throw new NotFoundException("Mülk bulunamadı");
        
        // Unit number benzersiz mi?
        if (!string.IsNullOrWhiteSpace(dto.UnitNumber))
        {
            var exists = await _context.Units
                .AnyAsync(u => u.PropertyId == propertyId && u.UnitNumber == dto.UnitNumber);
                
            if (exists)
                throw new BusinessException("Bu birim numarası zaten kullanılıyor");
        }
        
        var unit = new Unit
        {
            Id = Guid.NewGuid(),
            PropertyId = propertyId,
            Name = dto.Name,
            UnitNumber = dto.UnitNumber,
            Floor = dto.Floor,
            Description = dto.Description,
            MaxAdults = dto.MaxAdults,
            MaxChildren = dto.MaxChildren,
            MaxInfants = dto.MaxInfants,
            BasePrice = dto.BasePrice,
            CurrencyCode = dto.CurrencyCode,
            Size = dto.Size,
            View = dto.View,
            RoomAmenities = JsonSerializer.Serialize(dto.RoomAmenities ?? new List<string>()),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Units.Add(unit);
        
        // Property'nin birim sayısını güncelle
        property.TotalUnitCount = await _context.Units
            .CountAsync(u => u.PropertyId == propertyId && u.IsActive);
        property.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return _mapper.Map<UnitDto>(unit);
    }
    
    /// <summary>
    /// GetByPropertyAsync methodunu çalıştırır.
    /// </summary>
    public async Task<List<UnitDto>> GetByPropertyAsync(Guid propertyId)
    {
        var units = await _context.Units
            .Where(u => u.PropertyId == propertyId && u.IsActive)
            .OrderBy(u => u.Floor)
            .ThenBy(u => u.UnitNumber)
            .ToListAsync();
            
        return _mapper.Map<List<UnitDto>>(units);
    }
    
    /// <summary>
    /// UpdateBasePriceAsync methodunu çalıştırır.
    /// </summary>
    public async Task<bool> UpdateBasePriceAsync(Guid id, decimal price, string currencyCode)
    {
        var unit = await _context.Units.FindAsync(id);
        if (unit == null)
            throw new NotFoundException("Birim bulunamadı");
            
        unit.BasePrice = price;
        unit.CurrencyCode = currencyCode;
        
        await _context.SaveChangesAsync();
        return true;
    }
}
