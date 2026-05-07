using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;

namespace TrimangoCalendar.Data.Repositories.Notification
{
    public class NotificationRepository : BaseRepository<Core.Entities.Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Core.Entities.Notification>> GetByTenantAsync(Guid tenantId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Where(n => n.TenantId == tenantId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// GetUnreadCountAsync methodunu çalıştırır.
        /// </summary>
        public async Task<int> GetUnreadCountAsync(Guid tenantId)
        {
            return await _dbSet
                .CountAsync(n => n.TenantId == tenantId && n.ReadAt == null);
        }

        /// <summary>
        /// MarkAsReadAsync methodunu çalıştırır.
        /// </summary>
        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _dbSet.FindAsync(notificationId);
            if (notification != null)
            {
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// MarkAllAsReadAsync methodunu çalıştırır.
        /// </summary>
        public async Task MarkAllAsReadAsync(Guid tenantId)
        {
            var unread = await _dbSet
                .Where(n => n.TenantId == tenantId && n.ReadAt == null)
                .ToListAsync();

            foreach (var n in unread)
                n.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Core.Entities.Notification>> GetPendingNotificationsAsync()
        {
            return await _dbSet
                .Where(n => n.Status == Core.Entities.NotificationStatus.Pending)
                .OrderBy(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();
        }

        /// <summary>
        /// UpdateStatusAsync methodunu çalıştırır.
        /// </summary>
        public async Task UpdateStatusAsync(Guid notificationId, string status, string errorMessage = null)
        {
            var notification = await _dbSet.FindAsync(notificationId);
            if (notification != null)
            {
                if (Enum.TryParse<Core.Entities.NotificationStatus>(status, true, out var parsed))
                {
                    notification.Status = parsed;
                }

                if (notification.Status == Core.Entities.NotificationStatus.Sent)
                    notification.SentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
