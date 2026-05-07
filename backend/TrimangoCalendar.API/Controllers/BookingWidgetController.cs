using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingWidgetController : BaseController
{
    private readonly IBookingEngineService _bookingEngine;
    
    public BookingWidgetController(IBookingEngineService bookingEngine)
    {
        _bookingEngine = bookingEngine;
    }
    
    [HttpPost]
    /// <summary>
    /// CreateWidget methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> CreateWidget([FromBody] CreateWidgetDto dto)
    {
        var tenantId = GetTenantId();
        var widget = await _bookingEngine.CreateWidgetAsync(dto.PropertyId, dto);
        return Ok(new { success = true, data = widget });
    }
    
    [HttpGet("{propertyId}")]
    /// <summary>
    /// GetWidgets methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> GetWidgets(Guid propertyId)
    {
        var widgets = await _bookingEngine.GetPropertyWidgetsAsync(propertyId);
        return Ok(new { success = true, data = widgets });
    }
    
    [HttpGet("embed/{widgetId}")]
    /// <summary>
    /// GetEmbedCode methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> GetEmbedCode(Guid widgetId)
    {
        var widget = await _bookingEngine.GetWidgetByIdAsync(widgetId);
        var embedCode = await _bookingEngine.GetWidgetEmbedCode(widget.WidgetKey);
        
        return Ok(new { success = true, data = new { widget, embedCode } });
    }
    
    [HttpPut("{widgetId}")]
    /// <summary>
    /// UpdateWidget methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> UpdateWidget(Guid widgetId, [FromBody] UpdateWidgetDto dto)
    {
        var widget = await _bookingEngine.UpdateWidgetAsync(widgetId, dto);
        return Ok(new { success = true, data = widget });
    }
    
    [HttpDelete("{widgetId}")]
    /// <summary>
    /// DeleteWidget methodunu çalıştırır.
    /// </summary>
    public async Task<IActionResult> DeleteWidget(Guid widgetId)
    {
        await _bookingEngine.DeleteWidgetAsync(widgetId);
        return Ok(new { success = true, message = "Widget silindi" });
    }
    
    [HttpGet("preview/{widgetKey}")]
    /// <summary>
    /// PreviewWidget methodunu çalıştırır.
    /// </summary>
    public IActionResult PreviewWidget(string widgetKey)
    {
        return Redirect($"/widget/{widgetKey}");
    }}