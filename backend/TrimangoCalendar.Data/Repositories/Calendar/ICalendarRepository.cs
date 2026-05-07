using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Calendar
{
    public interface ICalendarRepository : IBaseRepository<CalendarBlock>
    {
        Task<IEnumerable<CalendarBlock>> GetBlocksByUnitAsync(Guid unitId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<CalendarBlock>> GetBlocksByPropertyAsync(Guid propertyId, DateTime startDate, DateTime endDate);
        Task<bool> HasOverlappingBlockAsync(Guid unitId, DateTime startDate, DateTime endDate, Guid? excludeBlockId = null);
        Task<IEnumerable<CalendarBlock>> GetActiveBlocksAsync(Guid unitId);
        Task UnblockDatesAsync(Guid blockId);
        Task UnblockAllExpiredAsync();
    }
}
