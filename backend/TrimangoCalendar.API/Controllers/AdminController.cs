using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrimangoCalendar.Data.Context;

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

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        await TrimangoCalendar.Data.SeedData.InitializeAsync(scope.ServiceProvider);
        return Success(message: "Veritabanı sıfırdan oluşturuldu ve seed işlemi başarıyla tamamlandı.");
    }
}
