public class ReportService : IReportService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    
    public ReportService(AppDbContext context, IMapper mapper, IMemoryCache cache)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
    }
    
    public async Task<DashboardDto> GetDashboardAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var today = DateTime.Today;
        var start = startDate ?? today.AddDays(-30);
        var end = endDate ?? today.AddDays(30);
        
        // Önbellekten kontrol et
        var cacheKey = $"dashboard_{tenantId}_{today:yyyyMMdd}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            
            var properties = await _context.Properties
                .Where(p => p.TenantId == tenantId && p.IsActive)
                .ToListAsync();
            
            var reservations = await _context.Reservations
                .Where(r => r.TenantId == tenantId && 
                           r.CheckIn >= start && r.CheckIn <= end)
                .ToListAsync();
            
            var todayReservations = reservations
                .Where(r => r.CheckIn.Date == today)
                .ToList();
            
            // Bugünkü durum
            var todayCheckIns = await _context.Reservations
                .CountAsync(r => r.TenantId == tenantId && 
                                r.Status == ReservationStatus.CheckedIn);
            
            var todayCheckOuts = await _context.Reservations
                .CountAsync(r => r.TenantId == tenantId && 
                                r.CheckOut.Date == today &&
                                r.Status == ReservationStatus.Confirmed);
            
            // Doluluk hesapla
            var totalUnits = await _context.Units
                .CountAsync(u => u.Property.TenantId == tenantId && u.IsActive);
            
            var occupiedUnits = todayCheckIns;
            var occupancyRate = totalUnits > 0 ? (double)occupiedUnits / totalUnits * 100 : 0;
            
            // Gelir hesapla
            var monthlyRevenue = reservations
                .Where(r => r.Status != ReservationStatus.Cancelled)
                .Sum(r => r.TotalAmount);
            
            var currencyCode = reservations.FirstOrDefault()?.CurrencyCode ?? "TRY";
            
            return new DashboardDto
            {
                // Bugün
                TodayCheckIns = todayCheckIns,
                TodayCheckOuts = todayCheckOuts,
                CurrentOccupancy = Math.Round(occupancyRate, 1),
                TotalUnits = totalUnits,
                OccupiedUnits = occupiedUnits,
                
                // Genel
                TotalProperties = properties.Count,
                TotalReservations = reservations.Count,
                ActiveReservations = reservations.Count(r => r.Status == ReservationStatus.Confirmed),
                PendingReservations = reservations.Count(r => r.Status == ReservationStatus.Pending),
                
                // Gelir
                MonthlyRevenue = monthlyRevenue,
                CurrencyCode = currencyCode,
                AverageRevenuePerReservation = reservations.Any() ? 
                    monthlyRevenue / reservations.Count(r => r.Status != ReservationStatus.Cancelled) : 0,
                
                // Son rezervasyonlar
                RecentReservations = await GetRecentReservations(tenantId, 10),
                
                // Doluluk grafiği (son 30 gün)
                OccupancyChart = await GetOccupancyChartData(tenantId, start, end),
                
                // Gelir grafiği
                RevenueChart = await GetRevenueChartData(tenantId, start, end),
                
                // En çok rezervasyon yapan acenteler
                TopAgencies = await GetTopAgencies(tenantId, start, end)
            };
        });
    }
    
    public async Task<OccupancyReportDto> GetOccupancyReportAsync(Guid tenantId, ReportRequestDto request)
    {
        var startDate = request.StartDate;
        var endDate = request.EndDate;
        
        var units = await _context.Units
            .Where(u => u.Property.TenantId == tenantId && u.IsActive)
            .ToListAsync();
        
        if (request.PropertyId.HasValue)
            units = units.Where(u => u.PropertyId == request.PropertyId.Value).ToList();
        
        var totalUnits = units.Count;
        var dailyOccupancy = new List<DailyOccupancyDto>();
        
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var reservedUnits = await _context.Reservations
                .CountAsync(r => units.Select(u => u.Id).Contains(r.UnitId) &&
                                r.CheckIn <= date && r.CheckOut > date &&
                                r.Status != ReservationStatus.Cancelled &&
                                r.Status != ReservationStatus.NoShow);
            
            dailyOccupancy.Add(new DailyOccupancyDto
            {
                Date = date,
                TotalUnits = totalUnits,
                ReservedUnits = reservedUnits,
                OccupancyRate = totalUnits > 0 ? Math.Round((double)reservedUnits / totalUnits * 100, 1) : 0
            });
        }
        
        // Excel export için
        await GenerateOccupancyExcel(tenantId, dailyOccupancy, request);
        
        return new OccupancyReportDto
        {
            PropertyId = request.PropertyId,
            StartDate = startDate,
            EndDate = endDate,
            TotalUnits = totalUnits,
            AverageOccupancyRate = dailyOccupancy.Any() ? 
                dailyOccupancy.Average(d => d.OccupancyRate) : 0,
            DailyOccupancy = dailyOccupancy,
            PeakOccupancyDate = dailyOccupancy.OrderByDescending(d => d.OccupancyRate).FirstOrDefault()?.Date,
            LowestOccupancyDate = dailyOccupancy.OrderBy(d => d.OccupancyRate).FirstOrDefault()?.Date
        };
    }
    
    public async Task<RevenueReportDto> GetRevenueReportAsync(Guid tenantId, ReportRequestDto request)
    {
        var reservations = await _context.Reservations
            .Include(r => r.Unit)
                .ThenInclude(u => u.Property)
            .Where(r => r.TenantId == tenantId &&
                       r.CreatedAt >= request.StartDate &&
                       r.CreatedAt <= request.EndDate &&
                       r.Status != ReservationStatus.Cancelled)
            .ToListAsync();
        
        if (request.PropertyId.HasValue)
            reservations = reservations.Where(r => r.Unit.PropertyId == request.PropertyId.Value).ToList();
        
        // Para birimine göre grupla
        var revenueByCurrency = reservations
            .GroupBy(r => r.CurrencyCode)
            .Select(g => new
            {
                Currency = g.Key,
                TotalRevenue = g.Sum(r => r.TotalAmount),
                Count = g.Count()
            });
        
        // Mülk bazında gelir
        var revenueByProperty = reservations
            .GroupBy(r => r.Unit.Property.Name)
            .Select(g => new PropertyRevenueDto
            {
                PropertyName = g.Key,
                TotalRevenue = g.Sum(r => r.TotalAmount),
                ReservationCount = g.Count(),
                AveragePerReservation = g.Average(r => r.TotalAmount),
                CurrencyCode = g.First().CurrencyCode
            })
            .OrderByDescending(r => r.TotalRevenue)
            .ToList();
        
        // Aylık gelir
        var monthlyRevenue = reservations
            .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
            .Select(g => new MonthlyRevenueDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalRevenue = g.Sum(r => r.TotalAmount),
                ReservationCount = g.Count(),
                CurrencyCode = g.First().CurrencyCode
            })
            .OrderBy(r => r.Year)
            .ThenBy(r => r.Month)
            .ToList();
        
        return new RevenueReportDto
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalRevenue = reservations.Sum(r => r.TotalAmount),
            CurrencyCode = reservations.FirstOrDefault()?.CurrencyCode ?? "TRY",
            TotalReservations = reservations.Count,
            AverageRevenuePerDay = reservations.Any() ? 
                reservations.Sum(r => r.TotalAmount) / (request.EndDate - request.StartDate).Days : 0,
            RevenueByCurrency = revenueByCurrency.Select(r => new CurrencyRevenueDto
            {
                CurrencyCode = r.Currency,
                TotalRevenue = r.TotalRevenue,
                Count = r.Count
            }).ToList(),
            RevenueByProperty = revenueByProperty,
            MonthlyRevenue = monthlyRevenue,
            TotalTax = reservations.Sum(r => r.TaxAmount),
            TotalServiceFee = reservations.Sum(r => r.ServiceFee),
            TotalDiscounts = reservations.Sum(r => r.DiscountAmount ?? 0)
        };
    }
    
    private async Task<List<RecentReservationDto>> GetRecentReservations(Guid tenantId, int count)
    {
        return await _context.Reservations
            .Include(r => r.Unit)
                .ThenInclude(u => u.Property)
            .Include(r => r.Guest)
            .Where(r => r.TenantId == tenantId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .Select(r => new RecentReservationDto
            {
                Id = r.Id,
                ReservationNumber = r.ReservationNumber,
                GuestName = r.Guest.FirstName + " " + r.Guest.LastName,
                PropertyName = r.Unit.Property.Name,
                UnitName = r.Unit.Name,
                CheckIn = r.CheckIn,
                CheckOut = r.CheckOut,
                TotalAmount = r.TotalAmount,
                CurrencyCode = r.CurrencyCode,
                Status = r.Status.ToString()
            })
            .ToListAsync();
    }
}
