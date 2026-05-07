namespace TrimangoCalendar.Core.Interfaces;

public interface IReportService
{
    Task<ReportDto> GenerateReportAsync(Guid tenantId, ReportRequestDto request);
    Task<ReportDto> GetReportAsync(Guid reportId);
    Task<List<ReportDto>> GetReportsAsync(Guid tenantId);
    Task<byte[]> DownloadReportAsync(Guid reportId);

    // Dashboard verileri
    Task<DashboardDto> GetDashboardAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null);
    Task<OccupancyReportDto> GetOccupancyReportAsync(Guid tenantId, ReportRequestDto request);
    Task<RevenueReportDto> GetRevenueReportAsync(Guid tenantId, ReportRequestDto request);
    Task<AgencyPerformanceDto> GetAgencyPerformanceAsync(Guid tenantId, Guid agencyId, DateTime start, DateTime end);
}

