public class TenantService : ITenantService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    
    public TenantService(AppDbContext context, IMapper mapper, IMemoryCache cache)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
    }
    
    public async Task<TenantDto> CreateAsync(CreateTenantDto dto)
    {
        // Subdomain oluştur
        var subdomain = GenerateSubdomain(dto.Name);
        
        // Subdomain benzersiz mi kontrol et
        if (!await IsSubdomainAvailable(subdomain))
        {
            subdomain = $"{subdomain}-{DateTime.Now:yyyyMMdd}";
        }
        
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Subdomain = subdomain,
            Email = dto.Email,
            Phone = dto.Phone,
            TaxNumber = dto.TaxNumber ?? string.Empty,
            TaxOffice = dto.TaxOffice ?? string.Empty,
            Address = dto.Address ?? string.Empty,
            City = dto.City ?? string.Empty,
            Country = dto.Country ?? "Türkiye",
            Plan = "Free",
            PlanStartDate = DateTime.UtcNow,
            MaxProperties = 5, // Free plan için
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        
        // Cache'i temizle
        _cache.Remove("all_tenants");
        
        return _mapper.Map<TenantDto>(tenant);
    }
    
    public async Task<TenantDto> GetByIdAsync(Guid id)
    {
        var tenant = await _context.Tenants
            .Include(t => t.Properties)
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (tenant == null)
            throw new NotFoundException($"Tenant bulunamadı: {id}");
            
        var dto = _mapper.Map<TenantDto>(tenant);
        dto.PropertyCount = tenant.Properties?.Count(p => p.IsActive) ?? 0;
        
        return dto;
    }
    
    public async Task<TenantDto> GetBySubdomainAsync(string subdomain)
    {
        string cacheKey = $"tenant_{subdomain}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(30);
            
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Subdomain == subdomain && t.IsActive);
                
            return tenant != null ? _mapper.Map<TenantDto>(tenant) : null;
        });
    }
    
    public async Task<List<TenantDto>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync("all_tenants", async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);
            
            var tenants = await _context.Tenants
                .Include(t => t.Properties)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
                
            return _mapper.Map<List<TenantDto>>(tenants);
        });
    }
    
    public async Task<bool> ChangePlanAsync(ChangePlanDto dto)
    {
        var tenant = await _context.Tenants.FindAsync(dto.TenantId);
        if (tenant == null)
            throw new NotFoundException("Tenant bulunamadı");
            
        var planLimits = GetPlanLimits(dto.NewPlan);
        
        tenant.Plan = dto.NewPlan;
        tenant.MaxProperties = planLimits.MaxProperties;
        tenant.PlanStartDate = DateTime.UtcNow;
        tenant.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        // Cache'i temizle
        _cache.Remove($"tenant_{tenant.Subdomain}");
        _cache.Remove("all_tenants");
        
        return true;
    }
    
    public async Task<bool> IsSubdomainAvailable(string subdomain)
    {
        return !await _context.Tenants.AnyAsync(t => t.Subdomain == subdomain);
    }
    
    private string GenerateSubdomain(string name)
    {
        // Türkçe karakterleri değiştir
        var subdomain = name.ToLower()
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace(",", "");
            
        // Alfanumerik olmayan karakterleri temizle
        subdomain = Regex.Replace(subdomain, @"[^a-z0-9\-]", "");
        
        return subdomain;
    }
    
    private (int MaxProperties, decimal MonthlyPrice) GetPlanLimits(string plan)
    {
        return plan switch
        {
            "Free" => (5, 0),
            "Pro" => (25, 49),
            "Enterprise" => (int.MaxValue, 199),
            _ => (5, 0)
        };
    }
}
