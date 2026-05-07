public interface IPropertyService
{
    Task<PropertyDto> CreateAsync(Guid tenantId, CreatePropertyDto dto);
    Task<PropertyDto> UpdateAsync(Guid id, UpdatePropertyDto dto);
    Task<PropertyDto> GetByIdAsync(Guid id);
    Task<PropertyDto> GetBySlugAsync(string slug);
    Task<List<PropertyDto>> GetByTenantAsync(Guid tenantId);
    Task<PaginatedResult<PropertyDto>> SearchAsync(PropertySearchDto search);
    Task<bool> ToggleActiveAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
    Task<decimal> GetStartingPriceAsync(Guid propertyId, string currencyCode);
    Task<bool> IsSlugAvailable(Guid tenantId, string slug);
}

