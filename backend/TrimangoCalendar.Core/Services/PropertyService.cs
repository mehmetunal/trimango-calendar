
// Core/Services/PropertyService.cs
public class PropertyService : IPropertyService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly IFileStorageService _fileStorage;
    
    public PropertyService(
        AppDbContext context,
        IMapper mapper,
        IMemoryCache cache,
        IFileStorageService fileStorage)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
        _fileStorage = fileStorage;
    }
    
    public async Task<PropertyDto> CreateAsync(Guid tenantId, CreatePropertyDto dto)
    {
        // Tenant mülk limitini kontrol et
        var tenant = await _context.Tenants.FindAsync(tenantId);
        var currentCount = await _context.Properties
            .CountAsync(p => p.TenantId == tenantId && p.IsActive);
            
        if (currentCount >= tenant.MaxProperties)
            throw new BusinessException($"Maksimum mülk sayısına ulaştınız: {tenant.MaxProperties}");
        
        var slug = await GenerateSlug(tenantId, dto.Name);
        
        var property = new Property
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Type = dto.Type,
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            ShortDescription = dto.ShortDescription,
            Email = dto.Email,
            Phone = dto.Phone,
            Website = dto.Website,
            Address = dto.Address,
            District = dto.District,
            City = dto.City,
            Country = dto.Country,
            PostalCode = dto.PostalCode,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            CheckInTime = TimeSpan.Parse(dto.CheckInTime ?? "14:00"),
            CheckOutTime = TimeSpan.Parse(dto.CheckOutTime ?? "12:00"),
            Amenities = JsonSerializer.Serialize(dto.Amenities ?? new List<string>()),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();
        
        // Cache temizle
        await ClearPropertyCache(tenantId);
        
        return _mapper.Map<PropertyDto>(property);
    }
    
    public async Task<PropertyDto> GetByIdAsync(Guid id)
    {
        var property = await _context.Properties
            .Include(p => p.Tenant)
            .Include(p => p.Units.Where(u => u.IsActive))
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (property == null)
            throw new NotFoundException("Mülk bulunamadı");
            
        var dto = _mapper.Map<PropertyDto>(property);
        dto.StartingPrice = property.Units?.Min(u => u.BasePrice) ?? 0;
        dto.TotalUnitCount = property.Units?.Count ?? 0;
        dto.TenantName = property.Tenant?.Name;
        
        return dto;
    }
    
    public async Task<PropertyDto> GetBySlugAsync(string slug)
    {
        string cacheKey = $"property_slug_{slug}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(30);
            
            var property = await _context.Properties
                .Include(p => p.Tenant)
                .Include(p => p.Units.Where(u => u.IsActive))
                .Include(p => p.Images.OrderBy(i => i.SortOrder))
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);
                
            if (property == null)
                return null;
                
            var dto = _mapper.Map<PropertyDto>(property);
            dto.StartingPrice = property.Units?.Min(u => u.BasePrice) ?? 0;
            dto.TotalUnitCount = property.Units?.Count ?? 0;
            
            return dto;
        });
    }
    
    public async Task<PaginatedResult<PropertyDto>> SearchAsync(PropertySearchDto search)
    {
        var query = _context.Properties
            .Include(p => p.Tenant)
            .Include(p => p.Units.Where(u => u.IsActive))
            .AsQueryable();
        
        // Filtreleme
        if (!string.IsNullOrWhiteSpace(search.City))
            query = query.Where(p => p.City.Contains(search.City));
            
        if (!string.IsNullOrWhiteSpace(search.Country))
            query = query.Where(p => p.Country == search.Country);
            
        if (search.Type.HasValue)
            query = query.Where(p => p.Type == search.Type.Value);
            
        if (search.Amenities?.Any() == true)
        {
            foreach (var amenity in search.Amenities)
            {
                query = query.Where(p => p.Amenities.Contains(amenity));
            }
        }
        
        // Fiyat filtresi (en düşük birim fiyatına göre)
        if (search.MinPrice.HasValue || search.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Units.Any(u => u.IsActive));
            
            if (search.MinPrice.HasValue)
                query = query.Where(p => p.Units.Min(u => u.BasePrice) >= search.MinPrice.Value);
                
            if (search.MaxPrice.HasValue)
                query = query.Where(p => p.Units.Min(u => u.BasePrice) <= search.MaxPrice.Value);
        }
        
        // Müsaitlik filtresi
        if (search.CheckIn.HasValue && search.CheckOut.HasValue)
        {
            query = query.Where(p => p.Units.Any(u => u.IsActive && 
                !u.Reservations.Any(r => 
                    r.Status != ReservationStatus.Cancelled &&
                    r.CheckIn < search.CheckOut.Value && 
                    r.CheckOut > search.CheckIn.Value)));
        }
        
        // Sıralama
        query = search.SortBy?.ToLower() switch
        {
            "price" => search.SortDescending 
                ? query.OrderByDescending(p => p.Units.Min(u => u.BasePrice))
                : query.OrderBy(p => p.Units.Min(u => u.BasePrice)),
            "rating" => search.SortDescending
                ? query.OrderByDescending(p => p.AverageRating)
                : query.OrderBy(p => p.AverageRating),
            "name" => search.SortDescending
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };
        
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((search.Page - 1) * search.PageSize)
            .Take(search.PageSize)
            .ToListAsync();
            
        var dtos = _mapper.Map<List<PropertyDto>>(items);
        
        return new PaginatedResult<PropertyDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = search.Page,
            PageSize = search.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)search.PageSize)
        };
    }
    
    private async Task<string> GenerateSlug(Guid tenantId, string name)
    {
        var slug = name.ToLower()
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace(" ", "-");
            
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        
        // Benzersizlik kontrolü
        var existingCount = await _context.Properties
            .CountAsync(p => p.TenantId == tenantId && p.Slug.StartsWith(slug));
            
        if (existingCount > 0)
            slug = $"{slug}-{existingCount + 1}";
            
        return slug;
    }
    
    private async Task ClearPropertyCache(Guid tenantId)
    {
        _cache.Remove($"properties_tenant_{tenantId}");
        // Diğer ilgili cache'leri temizle
    }
}

// Core/Interfaces/IFileStorageService.cs
public interface IFileStorageService
{
    Task<string> UploadAsync(IFormFile file, string folder);
    Task<bool> DeleteAsync(string filePath);
    string GetFileUrl(string filePath);
}

// Infrastructure/Services/LocalFileStorageService.cs
public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly string _basePath;
    
    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _env = env;
        _basePath = Path.Combine(env.WebRootPath, "uploads");
        
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }
    
    public async Task<string> UploadAsync(IFormFile file, string folder)
    {
        var folderPath = Path.Combine(_basePath, folder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
            
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(folderPath, fileName);
        
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        
        return Path.Combine("uploads", folder, fileName).Replace("\\", "/");
    }
    
    public Task<bool> DeleteAsync(string filePath)
    {
        var fullPath = Path.Combine(_env.WebRootPath, filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
    
    public string GetFileUrl(string filePath)
    {
        return $"/{filePath.Replace("\\", "/")}";
    }
}