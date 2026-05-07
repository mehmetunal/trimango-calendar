using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Notification
{
    public interface INotificationRepository : IBaseRepository<Core.Entities.Notification>
    {
        Task<IEnumerable<Core.Entities.Notification>> GetByTenantAsync(Guid tenantId, int page = 1, int pageSize = 20);
        Task<int> GetUnreadCountAsync(Guid tenantId);
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadAsync(Guid tenantId);
        Task<IEnumerable<Core.Entities.Notification>> GetPendingNotificationsAsync();
        Task UpdateStatusAsync(Guid notificationId, string status, string errorMessage = null);
    }
}
