namespace TrimangoCalendar.Core.Interfaces;

public interface IUnitService
{
    Task<UnitDto> CreateAsync(Guid propertyId, CreateUnitDto dto);
    Task<UnitDto> UpdateAsync(Guid id, UpdateUnitDto dto);
    Task<UnitDto> GetByIdAsync(Guid id);
    Task<List<UnitDto>> GetByPropertyAsync(Guid propertyId);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ToggleActiveAsync(Guid id);
    Task<bool> UpdateBasePriceAsync(Guid id, decimal price, string currencyCode);
}

