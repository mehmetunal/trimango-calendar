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
Bildirim ve raporlama modülü tamamlandı! Bu modül şunları içeriyor:

✅ Çok kanallı bildirim sistemi (Email, SMS, InApp, Push)

✅ Şablon tabanlı bildirimler

✅ Bildirim tercihleri yönetimi

✅ Dashboard (doluluk, gelir, check-in/out)

✅ Doluluk raporu (günlük, haftalık, aylık)

✅ Gelir raporu (mülk, acente, para birimi bazlı)

✅ Grafik verileri

✅ Excel export altyapısı

Şimdi sırada son modül olan MODÜL 7: MİSAFİR PORTALI & BOOKING ENGINE var. Misafirlerin online rezervasyon yapabileceği widget ve sayfaları içerecek. Devam edelim mi?

devam edelim
Harika! Son modüle geldik. Misafirlerin online rezervasyon yapabileceği booking engine ve misafir portalını hazırlayalım.

📦 MODÜL 7: BOOKING ENGINE & MİSAFİR PORTALI
