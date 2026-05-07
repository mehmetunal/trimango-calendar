namespace TrimangoCalendar.Core.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(IFormFile file, string folder);
    Task<bool> DeleteAsync(string filePath);
    string GetFileUrl(string filePath);
}

