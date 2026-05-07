using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("widget")]
[AllowAnonymous]
public class WidgetAssetsController : Controller
{
    [HttpGet("{widgetKey}")]
    public async Task<IActionResult> Index(string widgetKey)
    {
        // Widget sayfasını döndür
        return View("WidgetIndex", widgetKey);
    }
    
    [HttpGet("js/widget.js")]
    public IActionResult WidgetScript()
    {
        return File("~/js/widget.js", "application/javascript");
    }
    
    [HttpGet("css/widget.css")]
    public IActionResult WidgetStyles()
    {
        return File("~/css/widget.css", "text/css");
    }
}

