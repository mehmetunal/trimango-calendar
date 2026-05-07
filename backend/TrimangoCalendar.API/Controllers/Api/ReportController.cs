using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;
    
    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }
    
    [HttpPost("occupancy")]
    public async Task<IActionResult> GetOccupancyReport([FromBody] ReportRequestDto request)
    {
        var tenantId = GetTenantId();
        var report = await _reportService.GetOccupancyReportAsync(tenantId, request);
        return Ok(new { success = true, data = report });
    }
    
    [HttpPost("revenue")]
    public async Task<IActionResult> GetRevenueReport([FromBody] ReportRequestDto request)
    {
        var tenantId = GetTenantId();
        var report = await _reportService.GetRevenueReportAsync(tenantId, request);
        return Ok(new { success = true, data = report });
    }
    
    [HttpGet("agency-performance/{agencyId}")]
    public async Task<IActionResult> GetAgencyPerformance(Guid agencyId, [FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var tenantId = GetTenantId();
        var performance = await _reportService.GetAgencyPerformanceAsync(tenantId, agencyId, start, end);
        return Ok(new { success = true, data = performance });
    }
    
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateReport([FromBody] ReportRequestDto request)
    {
        var tenantId = GetTenantId();
        var report = await _reportService.GenerateReportAsync(tenantId, request);
        return Ok(new { success = true, data = report });
    }
    
    [HttpGet("download/{reportId}")]
    public async Task<IActionResult> DownloadReport(Guid reportId)
    {
        var fileBytes = await _reportService.DownloadReportAsync(reportId);
        var report = await _reportService.GetReportAsync(reportId);
        
        return File(fileBytes, "application/octet-stream", $"{report.Name}_{DateTime.Now:yyyyMMdd}.xlsx");
    }
    
    private Guid GetTenantId()
    {
        return (Guid)HttpContext.Items["TenantId"];
    }
}