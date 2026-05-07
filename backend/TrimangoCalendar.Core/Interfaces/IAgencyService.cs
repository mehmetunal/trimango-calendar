namespace TrimangoCalendar.Core.Interfaces;

public interface IAgencyService
{
    // Acente CRUD
    Task<AgencyDto> CreateAgencyAsync(Guid tenantId, CreateAgencyDto dto);
    Task<AgencyDto> GetAgencyByIdAsync(Guid id);
    Task<List<AgencyDto>> GetAgenciesAsync();
    Task<List<AgencyDto>> SearchAgenciesAsync(string searchTerm);
    
    // Yetkilendirme
    Task<AuthorizationDto> GrantAuthorizationAsync(Guid ownerTenantId, GrantAuthorizationDto dto);
    Task<AuthorizationDto> UpdateAuthorizationAsync(Guid authId, UpdateAuthorizationDto dto);
    Task RevokeAuthorizationAsync(Guid authId);
    Task<List<AuthorizationDto>> GetPropertyAuthorizationsAsync(Guid propertyId);
    Task<List<AuthorizationDto>> GetAgencyAuthorizationsAsync(Guid agencyId);
    
    // Acente paneli için
    Task<List<AuthorizedPropertyDto>> GetAgencyPropertiesAsync(Guid agencyId);
    Task<AuthorizedPropertyDetailDto> GetAgencyPropertyDetailAsync(Guid agencyId, Guid propertyId);
    
    // Kontenjan yönetimi
    Task UpdateAllotmentAsync(Guid authId, int totalAllotment);
    Task<bool> CheckAllotmentAvailabilityAsync(Guid authId);
    Task<bool> CanSetPriceAsync(Guid agencyId, Guid unitId);
}
