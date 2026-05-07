[ApiController]
[Route("api/[controller]")]
public class PropertyController : ControllerBase
{
    private readonly IPropertyService _propertyService;
    private readonly IUnitService _unitService;
    private readonly IImageService _imageService;
    
    public PropertyController(
        IPropertyService propertyService,
        IUnitService unitService,
        IImageService imageService)
    {
        _propertyService = propertyService;
        _unitService = unitService;
        _imageService = imageService;
    }
    
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] PropertySearchDto search)
    {
        var result = await _propertyService.SearchAsync(search);
        return Ok(new { success = true, data = result });
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var property = await _propertyService.GetByIdAsync(id);
            
            // Para birimi dönüşümü (varsayılan TRY)
            var currency = Request.Query["currency"].FirstOrDefault() ?? "TRY";
            
            return Ok(new { success = true, data = property, currency });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }
    
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var property = await _propertyService.GetBySlugAsync(slug);
        
        if (property == null)
            return NotFound(new { success = false, message = "Mülk bulunamadı" });
            
        return Ok(new { success = true, data = property });
    }
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreatePropertyDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var tenantId = GetTenantId();
        
        try
        {
            var property = await _propertyService.CreateAsync(tenantId, dto);
            return CreatedAtAction(nameof(GetById), new { id = property.Id }, 
                new { success = true, data = property });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    
    [HttpPost("{propertyId}/images")]
    [Authorize]
    public async Task<IActionResult> UploadImages(Guid propertyId, List<IFormFile> files)
    {
        var result = await _imageService.UploadPropertyImagesAsync(propertyId, files);
        return Ok(new { success = true, data = result });
    }
    
    [HttpPost("{propertyId}/units")]
    [Authorize]
    public async Task<IActionResult> CreateUnit(Guid propertyId, [FromBody] CreateUnitDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        try
        {
            var unit = await _unitService.CreateAsync(propertyId, dto);
            return Ok(new { success = true, data = unit });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (BusinessException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    
    [HttpGet("{propertyId}/units")]
    public async Task<IActionResult> GetUnits(Guid propertyId)
    {
        var units = await _unitService.GetByPropertyAsync(propertyId);
        
        // Fiyatları istenen para birimine çevir
        var currency = Request.Query["currency"].FirstOrDefault() ?? "TRY";
        
        return Ok(new { success = true, data = units, currency });
    }
    
    private Guid GetTenantId()
    {
        return (Guid)HttpContext.Items["TenantId"];
    }
}
