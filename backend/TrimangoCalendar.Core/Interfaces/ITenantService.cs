namespace TrimangoCalendar.Core.Interfaces;

public interface ITenantService
{
    Task<TenantDto> CreateAsync(CreateTenantDto dto);
    Task<TenantDto> UpdateAsync(Guid id, UpdateTenantDto dto);
    Task<TenantDto> GetByIdAsync(Guid id);
    Task<TenantDto> GetBySubdomainAsync(string subdomain);
    Task<List<TenantDto>> GetAllAsync();
    Task<bool> ToggleActiveAsync(Guid id);
    Task<bool> ChangePlanAsync(ChangePlanDto dto);
    Task<bool> IsSubdomainAvailable(string subdomain);
    Task<int> GetTenantCount();
}

