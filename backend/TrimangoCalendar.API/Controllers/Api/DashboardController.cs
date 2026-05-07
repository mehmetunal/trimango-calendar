using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly INotificationService _notificationService;
    
    public DashboardController(IReportService reportService, INotificationService notificationService)
    {
        _reportService = reportService;
        _notificationService = notificationService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetDashboard([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var tenantId = GetTenantId();
        var dashboard = await _reportService.GetDashboardAsync(tenantId, startDate, endDate);
        return Ok(new { success = true, data = dashboard });
    }
    
    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var tenantId = GetTenantId();
        var notifications = await _notificationService.GetNotificationsAsync(tenantId, page, pageSize);
        var unreadCount = await _notificationService.GetUnreadCountAsync(tenantId);
        
        return Ok(new { success = true, data = notifications, unreadCount });
    }
    
    [HttpPost("notifications/{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return Ok(new { success = true });
    }
    
    [HttpPost("notifications/read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var tenantId = GetTenantId();
        await _notificationService.MarkAllAsReadAsync(tenantId);
        return Ok(new { success = true });
    }
}

