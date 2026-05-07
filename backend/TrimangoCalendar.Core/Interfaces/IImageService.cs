// backend/TrimangoCalendar.Core/Interfaces/IImageService.cs
namespace TrimangoCalendar.Core.Interfaces
{
    public interface IImageService
    {
        // Mülk fotoğrafları
        Task<List<PropertyImageDto>> GetPropertyImagesAsync(Guid propertyId);
        Task<List<PropertyImageDto>> UploadPropertyImagesAsync(Guid propertyId, List<IFormFile> files);
        Task<bool> DeletePropertyImageAsync(Guid imageId);
        Task<PropertyImageDto> SetMainImageAsync(Guid imageId);
        Task<bool> ReorderImagesAsync(Guid propertyId, List<Guid> imageIds);

        // Birim fotoğrafları
        Task<List<UnitImageDto>> GetUnitImagesAsync(Guid unitId);
        Task<List<UnitImageDto>> UploadUnitImagesAsync(Guid unitId, List<IFormFile> files);
        Task<bool> DeleteUnitImageAsync(Guid imageId);

        // Genel
        Task<byte[]> GetImageBytesAsync(string filePath);
        Task<string> GetImageUrlAsync(string filePath, int? width = null, int? height = null);
        Task<bool> ImageExistsAsync(string filePath);
        Task<long> GetTotalStorageSizeAsync(Guid tenantId);

        // Optimizasyon
        Task<string> OptimizeImageAsync(string filePath, int maxWidth = 1920, int maxHeight = 1080, int quality = 85);
        Task<string> CreateThumbnailAsync(string filePath, int width = 300, int height = 200);

        // Validasyon
        bool IsValidImageFile(IFormFile file);
        bool IsValidImageSize(IFormFile file, long maxSizeInBytes = 5 * 1024 * 1024); // 5MB default
        List<string> GetAllowedExtensions();
    }
}