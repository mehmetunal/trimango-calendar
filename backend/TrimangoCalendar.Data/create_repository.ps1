# ============================================
# TrimangoCalendar.Data - Repository Katmanı
# Tüm Repository sınıfları ve altyapı
# ============================================

Write-Host "📦 TrimangoCalendar.Data Repository Katmanı Oluşturuluyor..." -ForegroundColor Cyan

# Ana dizini belirle (Masaüstünde TrimangoCalendar varsa oraya, yoksa yeni oluştur)
$root = "C:\Users\$env:USERNAME\Desktop\TrimangoCalendar"
if (-not (Test-Path $root)) {
    $root = "$PWD\TrimangoCalendar"
    New-Item -ItemType Directory -Force -Path $root | Out-Null
}

$dataPath = "$root\backend\TrimangoCalendar.Data"
$repositoryPath = "$dataPath\Repositories"

# Klasörleri oluştur
$folders = @(
    "$repositoryPath\Base",
    "$repositoryPath\Tenant",
    "$repositoryPath\Property", 
    "$repositoryPath\Unit",
    "$repositoryPath\Reservation",
    "$repositoryPath\Guest",
    "$repositoryPath\Agency",
    "$repositoryPath\Pricing",
    "$repositoryPath\Calendar",
    "$repositoryPath\Notification",
    "$repositoryPath\Report",
    "$repositoryPath\Widget",
    "$repositoryPath\Extensions",
    "$dataPath\Interfaces",
    "$dataPath\UnitOfWork"
)

foreach ($folder in $folders) {
    New-Item -ItemType Directory -Force -Path $folder | Out-Null
}

Write-Host "✅ Klasörler oluşturuldu" -ForegroundColor Green

# ============================================
# 1. BASE REPOSITORY
# ============================================
Write-Host "📝 Base Repository yazılıyor..." -ForegroundColor Yellow

# IBaseRepository.cs
@'
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TrimangoCalendar.Data.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
        Task<int> SaveChangesAsync();
    }
}
'@ | Out-File -FilePath "$repositoryPath\Base\IBaseRepository.cs" -Encoding UTF8

# BaseRepository.cs
@'
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Base
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly AppDbConext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(AppDbConext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();
            return await _dbSet.CountAsync(predicate);
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public virtual async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
'@ | Out-File -FilePath "$repositoryPath\Base\BaseRepository.cs" -Encoding UTF8

# ============================================
# 2. TENANT REPOSITORY
# ============================================
Write-Host "📝 Tenant Repository yazılıyor..." -ForegroundColor Yellow

# ITenantRepository.cs
@'
using System;
using System.Threading.Tasks;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Tenant
{
    public interface ITenantRepository : IBaseRepository<Core.Entities.Tenant>
    {
        Task<Core.Entities.Tenant> GetBySubdomainAsync(string subdomain);
        Task<Core.Entities.Tenant> GetByEmailAsync(string email);
        Task<bool> IsSubdomainAvailableAsync(string subdomain);
        Task<bool> IsEmailAvailableAsync(string email);
        Task<Core.Entities.Tenant> GetWithPropertiesAsync(Guid tenantId);
    }
}
'@ | Out-File -FilePath "$repositoryPath\Tenant\ITenantRepository.cs" -Encoding UTF8

# TenantRepository.cs
@'
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;

namespace TrimangoCalendar.Data.Repositories.Tenant
{
    public class TenantRepository : BaseRepository<Core.Entities.Tenant>, ITenantRepository
    {
        public TenantRepository(AppDbConext context) : base(context) { }

        public async Task<Core.Entities.Tenant> GetBySubdomainAsync(string subdomain)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Subdomain == subdomain && t.IsActive);
        }

        public async Task<Core.Entities.Tenant> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Email == email && t.IsActive);
        }

        public async Task<bool> IsSubdomainAvailableAsync(string subdomain)
        {
            return !await _dbSet.AnyAsync(t => t.Subdomain == subdomain);
        }

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            return !await _dbSet.AnyAsync(t => t.Email == email);
        }

        public async Task<Core.Entities.Tenant> GetWithPropertiesAsync(Guid tenantId)
        {
            return await _dbSet
                .Include(t => t.Properties)
                .FirstOrDefaultAsync(t => t.Id == tenantId);
        }
    }
}
'@ | Out-File -FilePath "$repositoryPath\Tenant\TenantRepository.cs" -Encoding UTF8

# ============================================
# 3. PROPERTY REPOSITORY
# ============================================
Write-Host "📝 Property Repository yazılıyor..." -ForegroundColor Yellow

# IPropertyRepository.cs
@'
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Property
{
    public interface IPropertyRepository : IBaseRepository<Core.Entities.Property>
    {
        Task<IEnumerable<Core.Entities.Property>> GetByTenantIdAsync(Guid tenantId);
        Task<Core.Entities.Property> GetBySlugAsync(Guid tenantId, string slug);
        Task<Core.Entities.Property> GetWithUnitsAsync(Guid propertyId);
        Task<Core.Entities.Property> GetFullDetailAsync(Guid propertyId);
        Task<IEnumerable<Core.Entities.Property>> SearchAsync(string city, string type, bool? isActive);
        Task<int> GetPropertyCountByTenantAsync(Guid tenantId);
    }
}
'@ | Out-File -FilePath "$repositoryPath\Property\IPropertyRepository.cs" -Encoding UTF8

# PropertyRepository.cs
@'
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;

namespace TrimangoCalendar.Data.Repositories.Property
{
    public class PropertyRepository : BaseRepository<Core.Entities.Property>, IPropertyRepository
    {
        public PropertyRepository(AppDbConext context) : base(context) { }

        public async Task<IEnumerable<Core.Entities.Property>> GetByTenantIdAsync(Guid tenantId)
        {
            return await _dbSet
                .Where(p => p.TenantId == tenantId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Core.Entities.Property> GetBySlugAsync(Guid tenantId, string slug)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Slug == slug);
        }

        public async Task<Core.Entities.Property> GetWithUnitsAsync(Guid propertyId)
        {
            return await _dbSet
                .Include(p => p.Units.Where(u => u.IsActive))
                .FirstOrDefaultAsync(p => p.Id == propertyId);
        }

        public async Task<Core.Entities.Property> GetFullDetailAsync(Guid propertyId)
        {
            return await _dbSet
                .Include(p => p.Units)
                .Include(p => p.Tenant)
                .FirstOrDefaultAsync(p => p.Id == propertyId);
        }

        public async Task<IEnumerable<Core.Entities.Property>> SearchAsync(string city, string type, bool? isActive)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(p => p.City.Contains(city));

            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<PropertyType>(type, out var propertyType))
                query = query.Where(p => p.Type == propertyType);

            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<int> GetPropertyCountByTenantAsync(Guid tenantId)
        {
            return await _dbSet.CountAsync(p => p.TenantId == tenantId && p.IsActive);
        }
    }
}
'@ | Out-File -FilePath "$repositoryPath\Property\PropertyRepository.cs" -Encoding UTF8

# ============================================
# 4. UNIT REPOSITORY
# ============================================
Write-Host "📝 Unit Repository yazılıyor..." -ForegroundColor Yellow

# IUnitRepository.cs
@'
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Unit
{
    public interface IUnitRepository : IBaseRepository<Core.Entities.Unit>
    {
        Task<IEnumerable<Core.Entities.Unit>> GetByPropertyIdAsync(Guid propertyId);
        Task<Core.Entities.Unit> GetByUnitNumberAsync(Guid propertyId, string unitNumber);
        Task<IEnumerable<Core.Entities.Unit>> GetActiveUnitsAsync(Guid propertyId);
        Task<decimal> GetMinPriceAsync(Guid propertyId);
        Task<decimal> GetMaxPriceAsync(Guid propertyId);
        Task<int> GetTotalCapacityAsync(Guid propertyId);
        Task<bool> IsUnitNumberExistsAsync(Guid propertyId, string unitNumber, Guid? excludeUnitId = null);
    }
}
'@ | Out-File -FilePath "$repositoryPath\Unit\IUnitRepository.cs" -Encoding UTF8

# UnitRepository.cs
@'
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;

namespace TrimangoCalendar.Data.Repositories.Unit
{
    public class UnitRepository : BaseRepository<Core.Entities.Unit>, IUnitRepository
    {
        public UnitRepository(AppDbConext context) : base(context) { }

        public async Task<IEnumerable<Core.Entities.Unit>> GetByPropertyIdAsync(Guid propertyId)
        {
            return await _dbSet
                .Where(u => u.PropertyId == propertyId)
                .OrderBy(u => u.Floor)
                .ThenBy(u => u.UnitNumber)
                .ToListAsync();
        }

        public async Task<Core.Entities.Unit> GetByUnitNumberAsync(Guid propertyId, string unitNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.PropertyId == propertyId && u.UnitNumber == unitNumber);
        }

        public async Task<IEnumerable<Core.Entities.Unit>> GetActiveUnitsAsync(Guid propertyId)
        {
            return await _dbSet
                .Where(u => u.PropertyId == propertyId && u.IsActive)
                .ToListAsync();
        }

        public async Task<decimal> GetMinPriceAsync(Guid propertyId)
        {
            var minPrice = await _dbSet
                .Where(u => u.PropertyId == propertyId && u.IsActive)
                .MinAsync(u => (decimal?)u.BasePrice);
            return minPrice ?? 0;
        }

        public async Task<decimal> GetMaxPriceAsync(Guid propertyId)
        {
            var maxPrice = await _dbSet
                .Where(u => u.PropertyId == propertyId && u.IsActive)
                .MaxAsync(u => (decimal?)u.BasePrice);
            return maxPrice ?? 0;
        }

        public async Task<int> GetTotalCapacityAsync(Guid propertyId)
        {
            var units = await _dbSet
                .Where(u => u.PropertyId == propertyId && u.IsActive)
                .ToListAsync();
            return units.Sum(u => u.MaxAdults + u.MaxChildren);
        }

        public async Task<bool> IsUnitNumberExistsAsync(Guid propertyId, string unitNumber, Guid? excludeUnitId = null)
        {
            var query = _dbSet.Where(u => u.PropertyId == propertyId && u.UnitNumber == unitNumber);
            if (excludeUnitId.HasValue)
                query = query.Where(u => u.Id != excludeUnitId.Value);
            return await query.AnyAsync();
        }
    }
}
'@ | Out-File -FilePath "$repositoryPath\Unit\UnitRepository.cs" -Encoding UTF8

# ============================================
# 5. RESERVATION REPOSITORY
# ============================================
Write-Host "📝 Reservation Repository yazılıyor..." -ForegroundColor Yellow

# IReservationRepository.cs
@'
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Reservation
{
    public interface IReservationRepository : IBaseRepository<Core.Entities.Reservation>
    {
        Task<IEnumerable<Core.Entities.Reservation>> GetByTenantIdAsync(Guid tenantId, int page = 1, int pageSize = 20);
        Task<Core.Entities.Reservation> GetByReservationNumberAsync(string reservationNumber);
        Task<Core.Entities.Reservation> GetFullDetailAsync(Guid reservationId);
        Task<IEnumerable<Core.Entities.Reservation>> GetByDateRangeAsync(Guid unitId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Core.Entities.Reservation>> GetUpcomingCheckInsAsync(Guid tenantId, DateTime date);
        Task<IEnumerable<Core.Entities.Reservation>> GetUpcomingCheckOutsAsync(Guid tenantId, DateTime date);
        Task<bool> IsUnitAvailableAsync(Guid unitId, DateTime checkIn, DateTime checkOut, Guid? excludeReservationId = null);
        Task<int> GetTotalCountByTenantAsync(Guid tenantId);
        Task<decimal> GetTotalRevenueAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetReservationCountByStatusAsync(Guid tenantId, ReservationStatus status);
    }
}
'@ | Out-File -FilePath "$repositoryPath\Reservation\IReservationRepository.cs" -Encoding UTF8

# ReservationRepository.cs
@'
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;

namespace TrimangoCalendar.Data.Repositories.Reservation
{
    public class ReservationRepository : BaseRepository<Core.Entities.Reservation>, IReservationRepository
    {
        public ReservationRepository(AppDbConext context) : base(context) { }

        public async Task<IEnumerable<Core.Entities.Reservation>> GetByTenantIdAsync(Guid tenantId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Include(r => r.Unit)
                    .ThenInclude(u => u.Property)
                .Include(r => r.Guest)
                .Where(r => r.TenantId == tenantId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Core.Entities.Reservation> GetByReservationNumberAsync(string reservationNumber)
        {
            return await _dbSet
                .Include(r => r.Unit)
                    .ThenInclude(u => u.Property)
                .Include(r => r.Guest)
                .FirstOrDefaultAsync(r => r.ReservationNumber == reservationNumber);
        }

        public async Task<Core.Entities.Reservation> GetFullDetailAsync(Guid reservationId)
        {
            return await _dbSet
                .Include(r => r.Unit)
                    .ThenInclude(u => u.Property)
                .Include(r => r.Guest)
                .Include(r => r.Tenant)
                .FirstOrDefaultAsync(r => r.Id == reservationId);
        }

        public async Task<IEnumerable<Core.Entities.Reservation>> GetByDateRangeAsync(Guid unitId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(r => r.UnitId == unitId
                    && r.Status != ReservationStatus.Cancelled
                    && r.Status != ReservationStatus.NoShow
                    && r.CheckIn < endDate
                    && r.CheckOut > startDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Entities.Reservation>> GetUpcomingCheckInsAsync(Guid tenantId, DateTime date)
        {
            return await _dbSet
                .Include(r => r.Unit)
                    .ThenInclude(u => u.Property)
                .Include(r => r.Guest)
                .Where(r => r.TenantId == tenantId
                    && r.CheckIn.Date == date.Date
                    && r.Status == ReservationStatus.Confirmed)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Entities.Reservation>> GetUpcomingCheckOutsAsync(Guid tenantId, DateTime date)
        {
            return await _dbSet
                .Include(r => r.Unit)
                    .ThenInclude(u => u.Property)
                .Include(r => r.Guest)
                .Where(r => r.TenantId == tenantId
                    && r.CheckOut.Date == date.Date
                    && r.Status == ReservationStatus.CheckedIn)
                .ToListAsync();
        }

        public async Task<bool> IsUnitAvailableAsync(Guid unitId, DateTime checkIn, DateTime checkOut, Guid? excludeReservationId = null)
        {
            var query = _dbSet.Where(r => r.UnitId == unitId
                && r.Status != ReservationStatus.Cancelled
                && r.Status != ReservationStatus.NoShow
                && r.CheckIn < checkOut
                && r.CheckOut > checkIn);

            if (excludeReservationId.HasValue)
                query = query.Where(r => r.Id != excludeReservationId.Value);

            return !await query.AnyAsync();
        }

        public async Task<int> GetTotalCountByTenantAsync(Guid tenantId)
        {
            return await _dbSet.CountAsync(r => r.TenantId == tenantId);
        }

        public async Task<decimal> GetTotalRevenueAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Where(r => r.TenantId == tenantId
                && r.Status != ReservationStatus.Cancelled);

            if (startDate.HasValue)
                query = query.Where(r => r.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(r => r.CreatedAt <= endDate.Value);

            return await query.SumAsync(r => r.TotalAmount);
        }

        public async Task<int> GetReservationCountByStatusAsync(Guid tenantId, ReservationStatus status)
        {
            return await _dbSet.CountAsync(r => r.TenantId == tenantId && r.Status == status);
        }
    }
}
'@ | Out-File -FilePath "$repositoryPath\Reservation\ReservationRepository.cs" -Encoding UTF8

# ============================================
# 6. GUEST REPOSITORY
# ============================================
Write-Host "📝 Guest Repository yazılıyor..." -ForegroundColor Yellow

# IGuestRepository.cs
@'
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Guest
{
    public interface IGuestRepository : IBaseRepository<Core.Entities.Guest>
    {
        Task<Core.Entities.Guest> GetByEmailAsync(Guid tenantId, string email);
        Task<Core.Entities.Guest> GetByPhoneAsync(Guid tenantId, string phone);
        Task<IEnumerable<Core.Entities.Guest>> GetByTenantIdAsync(Guid tenantId, int page = 1, int pageSize = 20);
        Task<IEnumerable<Core.Entities.Guest>> SearchAsync(Guid tenantId, string searchTerm);
        Task<Core.Entities.Guest> GetWithReservationsAsync(Guid guestId);
        Task<int> GetTotalGuestsByTenantAsync(Guid tenantId);
        Task<int> GetReturningGuestsCountAsync(Guid tenantId);
    }
}
'@ | Out-File -FilePath "$repositoryPath\Guest\IGuestRepository.cs" -Encoding UTF8

# GuestRepository.cs
@'
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;

namespace TrimangoCalendar.Data.Repositories.Guest
{
    public class GuestRepository : BaseRepository<Core.Entities.Guest>, IGuestRepository
    {
        public GuestRepository(AppDbConext context) : base(context) { }

        public async Task<Core.Entities.Guest> GetByEmailAsync(Guid tenantId, string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(g => g.TenantId == tenantId && g.Email == email);
        }

        public async Task<Core.Entities.Guest> GetByPhoneAsync(Guid tenantId, string phone)
        {
            return await _dbSet
                .FirstOrDefaultAsync(g => g.TenantId == tenantId && g.Phone == phone);
        }

        public async Task<IEnumerable<Core.Entities.Guest>> GetByTenantIdAsync(Guid tenantId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Where(g => g.TenantId == tenantId)
                .OrderByDescending(g => g.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Entities.Guest>> SearchAsync(Guid tenantId, string searchTerm)
        {
            return await _dbSet
                .Where(g => g.TenantId == tenantId &&
                    (g.FirstName.Contains(searchTerm) ||
                     g.LastName.Contains(searchTerm) ||
                     g.Email.Contains(searchTerm) ||
                     g.Phone.Contains(searchTerm)))
                .OrderByDescending(g => g.CreatedAt)
                .Take(20)
                .ToListAsync();
        }

        public async Task<Core.Entities.Guest> GetWithReservationsAsync(Guid guestId)
        {
            return await _dbSet
                .Include(g => g.Reservations)
                .FirstOrDefaultAsync(g => g.Id == guestId);
        }

        public async Task<int> GetTotalGuestsByTenantAsync(Guid tenantId)
        {
            return await _dbSet.CountAsync(g => g.TenantId == tenantId);
        }

        public async Task<int> GetReturningGuestsCountAsync(Guid tenantId)
        {
            return await _dbSet
                .Where(g => g.TenantId == tenantId)
                .CountAsync(g => g.Reservations.Count > 1);
        }
    }
}
'@ | Out-File -FilePath "$repositoryPath\Guest\GuestRepository.cs" -Encoding UTF8

# ============================================
# 7. AGENCY REPOSITORY
# ============================================
Write-Host "📝 Agency Repository yazılıyor..." -ForegroundColor Yellow

# IAgencyRepository.cs
@'
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Agency
{
    public interface IAgencyRepository : IBaseRepository<Core.Entities.Agency>
    {
        Task<IEnumerable<Core.Entities.Agency>> GetAllActiveAsync();
        Task<Core.Entities.Agency> GetByEmailAsync(string email);
        Task<IEnumerable<Core.Entities.Agency>> SearchAsync(string searchTerm);
        Task<Core.Entities.AgencyAuthorization> GetAuthorizationAsync(Guid agencyId, Guid propertyId);
        Task<IEnumerable<Core.Entities.AgencyAuthorization>> GetAuthorizationsByAgencyAsync(Guid agencyId);
        Task<IEnumerable<Core.Entities.AgencyAuthorization>> GetAuthorizationsByPropertyAsync(Guid propertyId);
        Task<bool> HasAuthorizationAsync(Guid agencyId, Guid propertyId);
        Task UpdateAllotmentUsageAsync(Guid authorizationId, int usedAllotment);
    }
}
'@ | Out-File -FilePath "$repositoryPath\Agency\IAgencyRepository.cs" -Encoding UTF8

# AgencyRepository.cs
@'
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;

namespace TrimangoCalendar.Data.Repositories.Agency
{
    public class AgencyRepository : BaseRepository<Core.Entities.Agency>, IAgencyRepository
    {
        public AgencyRepository(AppDbConext context) : base(context) { }

        public async Task<IEnumerable<Core.Entities.Agency>> GetAllActiveAsync()
        {
            return await _dbSet
                .Where(a => a.IsActive)
                .OrderBy(a => a.CompanyName)
                .ToListAsync();
        }

        public async Task<Core.Entities.Agency> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<IEnumerable<Core.Entities.Agency>> SearchAsync(string searchTerm)
        {
            return await _dbSet
                .Where(a => a.CompanyName.Contains(searchTerm) ||
                           a.Email.Contains(searchTerm) ||
                           a.ContactPerson.Contains(searchTerm))
                .OrderBy(a => a.CompanyName)
                .Take(20)
                .ToListAsync();
        }

        public async Task<Core.Entities.AgencyAuthorization> GetAuthorizationAsync(Guid agencyId, Guid propertyId)
        {
            return await _context.AgencyAuthorizations
                .Include(a => a.Agency)
                .Include(a => a.Property)
                .FirstOrDefaultAsync(a => a.AgencyId == agencyId && a.PropertyId == propertyId && a.IsActive);
        }

        public async Task<IEnumerable<Core.Entities.AgencyAuthorization>> GetAuthorizationsByAgencyAsync(Guid agencyId)
        {
            return await _context.AgencyAuthorizations
                .Include(a => a.Agency)
                .Include(a => a.Property)
                .Where(a => a.AgencyId == agencyId && a.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Core.Entities.AgencyAuthorization>> GetAuthorizationsByPropertyAsync(Guid propertyId)
        {
            return await _context.AgencyAuthorizations
                .Include(a => a.Agency)
                .Include(a => a.Property)
                .Where(a => a.PropertyId == propertyId && a.IsActive)
                .ToListAsync();
        }

        public async Task<bool> HasAuthorizationAsync(Guid agencyId, Guid propertyId)
        {
            return await _context.AgencyAuthorizations
                .AnyAsync(a => a.AgencyId == agencyId && a.PropertyId == propertyId && a.IsActive);
        }

        public async Task UpdateAllotmentUsageAsync(Guid authorizationId, int usedAllotment)
        {
            var auth = await _context.AgencyAuthorizations.FindAsync(authorizationId);
            if (auth != null)
            {
                auth.UsedAllotment = usedAllotment;
                await _context.SaveChangesAsync();
            }
        }
    }
}
'@ | Out-File -FilePath "$repositoryPath\Agency\AgencyRepository.cs" -Encoding UTF8

# ============================================
# 8. PRICING REPOSITORY
# ============================================
Write-Host "📝 Pricing Repository yazılıyor..." -ForegroundColor Yellow

# IPricingRepository.cs
@'
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Interfaces;

namespace TrimangoCalendar.Data.Repositories.Pricing
{
    public interface IPricingRepository : IBaseRepository<SeasonRate>
    {
        Task<IEnumerable<SeasonRate>> GetByUnitIdAsync(Guid unitId);
        Task<SeasonRate> GetActiveRateAsync(Guid unitId, DateTime date);
        Task<IEnumerable<SeasonRate>> GetOverlappingRatesAsync(Guid unitId, DateTime startDate, DateTime endDate);
        Task<decimal> GetExchangeRateAsync(string baseCurrency, string targetCurrency, DateTime date);
        Task<IEnumerable<ExchangeRate>> GetExchangeRatesForDateAsync(DateTime date);
        Task<IEnumerable<Currency>> GetActiveCurrenciesAsync();
        Task UpdateExchangeRatesAsync(IEnumerable<ExchangeRate> rates);
    }
}
'@ | Out-File -FilePath "$repositoryPath\Pricing\IPricingRepository.cs" -Encoding UTF8

# PricingRepository.cs
@'
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;

namespace TrimangoCalendar.Data.Repositories.Pricing
{
    public class PricingRepository : BaseRepository<SeasonRate>, IPricingRepository
    {
        private readonly AppDbConext _context;

        public PricingRepository(AppDbConext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SeasonRate>> GetByUnitIdAsync(Guid unitId)
        {
            return await _dbSet
                .Where(s => s.UnitId == unitId && s.IsActive)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<SeasonRate> GetActiveRateAsync(Guid unitId, DateTime date)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.UnitId == unitId
                    && s.IsActive
                    && s.StartDate <= date
                    && s.EndDate >= date);
        }

        public async Task<IEnumerable<SeasonRate>> GetOverlappingRatesAsync(Guid unitId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(s => s.UnitId == unitId
                    && s.IsActive
                    && s.StartDate < endDate
                    && s.EndDate > startDate)
                .ToListAsync();
        }

        public async Task<decimal> GetExchangeRateAsync(string baseCurrency, string targetCurrency, DateTime date)
        {
            var rate = await _context.ExchangeRates
                .FirstOrDefaultAsync(r => r.BaseCurrencyCode == baseCurrency
                    && r.TargetCurrencyCode == targetCurrency
                    && r.Date == date);

            if (rate != null) return rate.Rate;

            // En yakın tarihli kuru bul
            var nearestRate = await _context.ExchangeRates
                .Where(r => r.BaseCurrencyCode == baseCurrency
                    && r.TargetCurrencyCode == targetCurrency)
                .OrderByDescending(r => r.Date)
                .FirstOrDefaultAsync();

            return nearestRate?.Rate ?? 1;
        }

        public async Task<IEnumerable<ExchangeRate>> GetExchangeRatesForDateAsync(DateTime date)
        {
            return await _context.ExchangeRates
                .Where(r => r.Date == date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Currency>> GetActiveCurrenciesAsync()
        {
            return await _context.Currencies
                .Where(c => c.IsActive)
                .OrderBy(c => c.Code)
                .ToListAsync();
        }

        public async Task UpdateExchangeRatesAsync(IEnumerable<ExchangeRate> rates)
        {
            await _context.ExchangeRates.AddRangeAsync(rates);
            await _context.SaveChangesAsync();
        }
    }
}
'@ | Out-File -FilePath "$repositoryPath\Pricing\PricingRepository.cs" -Encoding UTF8

# ============================================
# 9. CALENDAR REPOSITORY
# ============================================
Write-Host "📝 Calendar Repository yazılıyor..." -ForegroundColor Yellow

# ICalendarRepository.cs
@'
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
'@ | Out-File -FilePath "$repositoryPath\Calendar\ICalendarRepository.cs" -Encoding UTF8

# CalendarRepository.cs
@'
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Base;

namespace TrimangoCalendar.Data.Repositories.Calendar
{
    public class CalendarRepository : BaseRepository<CalendarBlock>, ICalendarRepository
    {
        public CalendarRepository(AppDbConext context) : base(context) { }

        public async Task<IEnumerable<CalendarBlock>> GetBlocksByUnitAsync(Guid unitId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(b => b.UnitId == unitId
                    && b.IsActive
                    && b.StartDate < endDate
                    && b.EndDate > startDate)
                .OrderBy(b => b.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CalendarBlock>> GetBlocksByPropertyAsync(Guid propertyId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(b => b.Unit)
                .Where(b => b.Unit.PropertyId == propertyId
                    && b.IsActive
                    && b.StartDate < endDate
                    && b.EndDate > startDate)
                .OrderBy(b => b.StartDate)
                .ToListAsync();
        }

        public async Task<bool> HasOverlappingBlockAsync(Guid unitId, DateTime startDate, DateTime endDate, Guid? excludeBlockId = null)
        {
            var query = _dbSet.Where(b => b.UnitId == unitId
                && b.IsActive
                && b.StartDate < endDate
                && b.EndDate > startDate);

            if (excludeBlockId.HasValue)
                query = query.Where(b => b.Id != excludeBlockId.Value);

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<CalendarBlock>> GetActiveBlocksAsync(Guid unitId)
        {
            return await _dbSet
                .Where(b => b.UnitId == unitId && b.IsActive)
                .OrderBy(b => b.StartDate)
                .ToListAsync();
        }

        public async Task UnblockDatesAsync(Guid blockId)
        {
            var block = await _dbSet.FindAsync(blockId);
            if (block != null)
            {
                block.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnblockAllExpiredAsync()
        {
            var expiredBlocks = await _dbSet
                .Where(b => b.IsActive && b.EndDate < DateTime.Today)
                .ToListAsync();

            foreach (var block in expiredBlocks)
                block.IsActive = false;

            await _context.SaveChangesAsync();
        }
    }
}
'@ | Out-File -FilePath "$repositoryPath\Calendar\CalendarRepository.cs" -Encoding UTF8

# ============================================
# 10. NOTIFICATION REPOSITORY
# ============================================
Write-Host "📝 Notification Repository yazılıyor..." -ForegroundColor Yellow

# INotificationRepository.cs
@'
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
'@ | Out-File -FilePath "$repositoryPath\Notification\INotificationRepository.cs" -Encoding UTF8

# NotificationRepository.cs
@'
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
        public NotificationRepository(AppDbConext context) : base(context) { }

        public async Task<IEnumerable<Core.Entities.Notification>> GetByTenantAsync(Guid tenantId, int page = 1, int pageSize = 20)
        {
            return await _dbSet
                .Where(n => n.TenantId == tenantId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid tenantId)
        {
            return await _dbSet
                .CountAsync(n => n.TenantId == tenantId && n.ReadAt == null);
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _dbSet.FindAsync(notificationId);
            if (notification != null)
            {
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

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
                .Where(n => n.Status == "Pending")
                .OrderBy(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();
        }

        public async Task UpdateStatusAsync(Guid notificationId, string status, string errorMessage = null)
        {
            var notification = await _dbSet.FindAsync(notificationId);
            if (notification != null)
            {
                notification.Status = status;
                if (status == "Sent")
                    notification.SentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
'@ | Out-File -FilePath "$repositoryPath\Notification\NotificationRepository.cs" -Encoding UTF8

# ============================================
# 11. UNIT OF WORK
# ============================================
Write-Host "📝 Unit of Work yazılıyor..." -ForegroundColor Yellow

# IUnitOfWork.cs
@'
using System;
using System.Threading.Tasks;
using TrimangoCalendar.Data.Repositories.Tenant;
using TrimangoCalendar.Data.Repositories.Property;
using TrimangoCalendar.Data.Repositories.Unit;
using TrimangoCalendar.Data.Repositories.Reservation;
using TrimangoCalendar.Data.Repositories.Guest;
using TrimangoCalendar.Data.Repositories.Agency;
using TrimangoCalendar.Data.Repositories.Pricing;
using TrimangoCalendar.Data.Repositories.Calendar;
using TrimangoCalendar.Data.Repositories.Notification;

namespace TrimangoCalendar.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        ITenantRepository Tenants { get; }
        IPropertyRepository Properties { get; }
        IUnitRepository Units { get; }
        IReservationRepository Reservations { get; }
        IGuestRepository Guests { get; }
        IAgencyRepository Agencies { get; }
        IPricingRepository Pricing { get; }
        ICalendarRepository Calendar { get; }
        INotificationRepository Notifications { get; }
        
        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
'@ | Out-File -FilePath "$dataPath\UnitOfWork\IUnitOfWork.cs" -Encoding UTF8

# UnitOfWork.cs
@'
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using TrimangoCalendar.Data.Context;
using TrimangoCalendar.Data.Repositories.Tenant;
using TrimangoCalendar.Data.Repositories.Property;
using TrimangoCalendar.Data.Repositories.Unit;
using TrimangoCalendar.Data.Repositories.Reservation;
using TrimangoCalendar.Data.Repositories.Guest;
using TrimangoCalendar.Data.Repositories.Agency;
using TrimangoCalendar.Data.Repositories.Pricing;
using TrimangoCalendar.Data.Repositories.Calendar;
using TrimangoCalendar.Data.Repositories.Notification;

namespace TrimangoCalendar.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbConext _context;
        private IDbContextTransaction _transaction;

        // Lazy loading for repositories
        private ITenantRepository _tenantRepository;
        private IPropertyRepository _propertyRepository;
        private IUnitRepository _unitRepository;
        private IReservationRepository _reservationRepository;
        private IGuestRepository _guestRepository;
        private IAgencyRepository _agencyRepository;
        private IPricingRepository _pricingRepository;
        private ICalendarRepository _calendarRepository;
        private INotificationRepository _notificationRepository;

        public UnitOfWork(AppDbConext context)
        {
            _context = context;
        }

        public ITenantRepository Tenants =>
            _tenantRepository ??= new TenantRepository(_context);

        public IPropertyRepository Properties =>
            _propertyRepository ??= new PropertyRepository(_context);

        public IUnitRepository Units =>
            _unitRepository ??= new UnitRepository(_context);

        public IReservationRepository Reservations =>
            _reservationRepository ??= new ReservationRepository(_context);

        public IGuestRepository Guests =>
            _guestRepository ??= new GuestRepository(_context);

        public IAgencyRepository Agencies =>
            _agencyRepository ??= new AgencyRepository(_context);

        public IPricingRepository Pricing =>
            _pricingRepository ??= new PricingRepository(_context);

        public ICalendarRepository Calendar =>
            _calendarRepository ??= new CalendarRepository(_context);

        public INotificationRepository Notifications =>
            _notificationRepository ??= new NotificationRepository(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
'@ | Out-File -FilePath "$dataPath\UnitOfWork\UnitOfWork.cs" -Encoding UTF8

# ============================================
# TAMAMLANDI
# ============================================
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "✅ Repository Katmanı Tamamlandı!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Oluşturulan Dosyalar:" -ForegroundColor Yellow
Write-Host ""
Write-Host "📁 Repositories/Base/" -ForegroundColor White
Write-Host "   ├── IBaseRepository.cs" -ForegroundColor Gray
Write-Host "   └── BaseRepository.cs" -ForegroundColor Gray
Write-Host ""
Write-Host "📁 Repositories/Tenant/" -ForegroundColor White
Write-Host "   ├── ITenantRepository.cs" -ForegroundColor Gray
Write-Host "   └── TenantRepository.cs" -ForegroundColor Gray
Write-Host ""
Write-Host "📁 Repositories/Property/" -ForegroundColor White
Write-Host "   ├── IPropertyRepository.cs" -ForegroundColor Gray
Write-Host "   └── PropertyRepository.cs" -ForegroundColor Gray
Write-Host ""
Write-Host "📁 Repositories/Unit/" -ForegroundColor White
Write-Host "   ├── IUnitRepository.cs" -ForegroundColor Gray
Write-Host "   └── UnitRepository.cs" -ForegroundColor Gray
Write-Host ""
Write-Host "📁 Repositories/Reservation/" -ForegroundColor White
Write-Host "   ├── IReservationRepository.cs" -ForegroundColor Gray
Write-Host "   └── ReservationRepository.cs" -ForegroundColor Gray
Write-Host ""
Write-Host "📁 Repositories/Guest/" -ForegroundColor White
Write-Host "   ├── IGuestRepository.cs" -ForegroundColor Gray
Write-Host "   └── GuestRepository.cs" -ForegroundColor Gray
Write-Host ""
Write-Host "📁 Repositories/Agency/" -ForegroundColor White
Write-Host "   ├── IAgencyRepository.cs" -ForegroundColor Gray
Write-Host "   └── AgencyRepository.cs" -ForegroundColor Gray
Write-Host ""
Write-Host "📁 Repositories/Pricing/" -ForegroundColor White
Write-Host "   ├── IPricingRepository.cs" -ForegroundColor Gray
Write-Host "   └── PricingRepository.cs" -ForegroundColor Gray
Write-Host ""
Write-Host "📁 Repositories/Calendar/" -ForegroundColor White
Write-Host "   ├── ICalendarRepository.cs" -ForegroundColor Gray
Write-Host "   └── CalendarRepository.cs" -ForegroundColor Gray
Write-Host ""
Write-Host "📁 Repositories/Notification/" -ForegroundColor White
Write-Host "   ├── INotificationRepository.cs" -ForegroundColor Gray
Write-Host "   └── NotificationRepository.cs" -ForegroundColor Gray
Write-Host ""
Write-Host "📁 UnitOfWork/" -ForegroundColor White
Write-Host "   ├── IUnitOfWork.cs" -ForegroundColor Gray
Write-Host "   └── UnitOfWork.cs" -ForegroundColor Gray
Write-Host ""
Write-Host "📝 Kullanım Örneği:" -ForegroundColor Yellow
Write-Host ""
Write-Host '  // Dependency Injection (Program.cs)' -ForegroundColor Gray
Write-Host '  builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();' -ForegroundColor White
Write-Host ""
Write-Host '  // Servis içinde kullanım' -ForegroundColor Gray
Write-Host '  public class PropertyService' -ForegroundColor White
Write-Host '  {' -ForegroundColor White
Write-Host '      private readonly IUnitOfWork _unitOfWork;' -ForegroundColor White
Write-Host '      public PropertyService(IUnitOfWork unitOfWork)' -ForegroundColor White
Write-Host '      {' -ForegroundColor White
Write-Host '          _unitOfWork = unitOfWork;' -ForegroundColor White
Write-Host '      }' -ForegroundColor White
Write-Host '  }' -ForegroundColor White
Write-Host ""
Write-Host "✅ Script başarıyla tamamlandı!" -ForegroundColor Green