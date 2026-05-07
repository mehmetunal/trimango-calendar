using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TrimangoCalendar.API.Controllers;

[ApiController]
[Route("api/admin")]
[AllowAnonymous]
public class AdminController : BaseController
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWebHostEnvironment _environment;

    public AdminController(IServiceProvider serviceProvider, IWebHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
    }

    /// <summary>
    /// SeedData işlemini manuel olarak tetikler.
    /// </summary>
    [HttpPost("seed")]
    public async Task<IActionResult> RunSeed()
    {
        if (!_environment.IsDevelopment())
        {
            return FailForbidden("Bu endpoint sadece Development ortamında kullanılabilir.");
        }

        await TrimangoCalendar.Data.SeedData.InitializeAsync(_serviceProvider);
        return Success(message: "Seed işlemi başarıyla tamamlandı.");
    }
}
