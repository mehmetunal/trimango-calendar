// Web/Controllers/Api/TenantController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Sadece admin tenant oluşturabilir
public class TenantController : ControllerBase
{
    private readonly ITenantService _tenantService;
    
    public TenantController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tenants = await _tenantService.GetAllAsync();
        return Ok(new { success = true, data = tenants });
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var tenant = await _tenantService.GetByIdAsync(id);
            return Ok(new { success = true, data = tenant });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var tenant = await _tenantService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, 
            new { success = true, data = tenant });
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        try
        {
            var tenant = await _tenantService.UpdateAsync(id, dto);
            return Ok(new { success = true, data = tenant });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }
    
    [HttpPost("change-plan")]
    public async Task<IActionResult> ChangePlan([FromBody] ChangePlanDto dto)
    {
        try
        {
            await _tenantService.ChangePlanAsync(dto);
            return Ok(new { success = true, message = "Plan başarıyla güncellendi" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }
    
    [HttpPost("check-subdomain")]
    public async Task<IActionResult> CheckSubdomain([FromBody] string subdomain)
    {
        var isAvailable = await _tenantService.IsSubdomainAvailable(subdomain);
        return Ok(new { available = isAvailable });
    }
    
    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var result = await _tenantService.ToggleActiveAsync(id);
        return Ok(new { success = true, isActive = result });
    }
}