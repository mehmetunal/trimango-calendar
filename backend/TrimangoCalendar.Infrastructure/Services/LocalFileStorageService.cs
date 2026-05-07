using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using TrimangoCalendar.Core.Interfaces;

namespace TrimangoCalendar.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly string _basePath;
    
    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _env = env;
        _basePath = Path.Combine(env.WebRootPath, "uploads");
        
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }
    
    /// <summary>
    /// UploadAsync methodunu çalıştırır.
    /// </summary>
    public async Task<string> UploadAsync(IFormFile file, string folder)
    {
        var folderPath = Path.Combine(_basePath, folder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
            
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(folderPath, fileName);
        
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        
        return Path.Combine("uploads", folder, fileName).Replace("\\", "/");
    }
    
    /// <summary>
    /// DeleteAsync methodunu çalıştırır.
    /// </summary>
    public Task<bool> DeleteAsync(string filePath)
    {
        var fullPath = Path.Combine(_env.WebRootPath, filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
    
    /// <summary>
    /// GetFileUrl methodunu çalıştırır.
    /// </summary>
    public string GetFileUrl(string filePath)
    {
        return $"/{filePath.Replace("\\", "/")}";
    }
}
