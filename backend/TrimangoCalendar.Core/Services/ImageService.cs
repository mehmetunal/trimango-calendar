// backend/TrimangoCalendar.Core/Services/ImageService.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using TrimangoCalendar.Core.DTOs;
using TrimangoCalendar.Core.Entities;
using TrimangoCalendar.Core.Interfaces;
using TrimangoCalendar.Data.Context;

namespace TrimangoCalendar.Core.Services
{
    public class ImageService : IImageService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ImageService> _logger;
        private readonly string _baseUploadPath;
        private readonly string _baseUrl;

        private static readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
        private static readonly string[] _allowedMimeTypes = { 
            "image/jpeg", "image/png", "image/gif", "image/webp", "image/bmp" 
        };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public ImageService(
            AppDbContext context,
            IWebHostEnvironment env,
            ILogger<ImageService> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
            _baseUploadPath = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads");
            _baseUrl = "/uploads";

            // Klasörleri oluştur
            EnsureDirectoriesExist();
        }

        #region Mülk Fotoğrafları

        /// <summary>
        /// GetPropertyImagesAsync methodunu çalıştırır.
        /// </summary>
        public async Task<List<PropertyImageDto>> GetPropertyImagesAsync(Guid propertyId)
        {
            return await _context.PropertyImages
                .Where(img => img.PropertyId == propertyId)
                .OrderBy(img => img.SortOrder)
                .Select(img => new PropertyImageDto
                {
                    Id = img.Id,
                    PropertyId = img.PropertyId,
                    FileName = img.FileName,
                    OriginalFileName = img.OriginalFileName,
                    FilePath = img.FilePath,
                    ThumbnailPath = img.ThumbnailPath,
                    FileSize = img.FileSize,
                    ContentType = img.ContentType,
                    SortOrder = img.SortOrder,
                    IsMain = img.IsMain,
                    UploadedAt = img.UploadedAt,
                    FileUrl = _baseUrl + "/" + img.FilePath.Replace("\\", "/"),
                    ThumbnailUrl = img.ThumbnailPath != null ? _baseUrl + "/" + img.ThumbnailPath.Replace("\\", "/") : null
                })
                .ToListAsync();
        }

        /// <summary>
        /// UploadPropertyImagesAsync methodunu çalıştırır.
        /// </summary>
        public async Task<List<PropertyImageDto>> UploadPropertyImagesAsync(Guid propertyId, List<IFormFile> files)
        {
            var property = await _context.Properties.FindAsync(propertyId);
            if (property == null)
                throw new NotFoundException("Mülk bulunamadı");

            if (files == null || files.Count == 0)
                throw new BusinessException("En az bir dosya seçilmelidir");

            var results = new List<PropertyImageDto>();
            var currentMaxOrder = await _context.PropertyImages
                .Where(img => img.PropertyId == propertyId)
                .MaxAsync(img => (int?)img.SortOrder) ?? 0;

            var hasMainImage = await _context.PropertyImages
                .AnyAsync(img => img.PropertyId == propertyId && img.IsMain);

            foreach (var file in files)
            {
                // Validasyon
                if (!IsValidImageFile(file))
                    throw new BusinessException($"Geçersiz dosya formatı: {file.FileName}");

                if (!IsValidImageSize(file))
                    throw new BusinessException($"Dosya boyutu çok büyük: {file.FileName} (Max: 10MB)");

                try
                {
                    currentMaxOrder++;
                    var isFirstImage = !hasMainImage && results.Count == 0 && currentMaxOrder == 1;

                    // Benzersiz dosya adı
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                    var propertyFolder = Path.Combine("properties", propertyId.ToString());

                    // Ana görseli kaydet
                    var folderPath = Path.Combine(_baseUploadPath, propertyFolder);
                    var filePath = Path.Combine(folderPath, uniqueFileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Thumbnail oluştur
                    var thumbnailFileName = $"thumb_{uniqueFileName}";
                    var thumbnailPath = Path.Combine(folderPath, thumbnailFileName);
                    
                    try
                    {
                        await CreateThumbnailFromPathAsync(filePath, thumbnailPath, 400, 300);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Thumbnail oluşturulamadı: {FileName}", file.FileName);
                        thumbnailPath = null;
                    }

                    // Veritabanına kaydet
                    var propertyImage = new PropertyImage
                    {
                        Id = Guid.NewGuid(),
                        PropertyId = propertyId,
                        FileName = uniqueFileName,
                        OriginalFileName = file.FileName,
                        FilePath = Path.Combine(propertyFolder, uniqueFileName),
                        ThumbnailPath = thumbnailPath != null ? Path.Combine(propertyFolder, thumbnailFileName) : null,
                        FileSize = file.Length,
                        ContentType = file.ContentType,
                        SortOrder = currentMaxOrder,
                        IsMain = isFirstImage,
                        UploadedAt = DateTime.UtcNow
                    };

                    _context.PropertyImages.Add(propertyImage);

                    // Mülkün kapak fotoğrafını güncelle
                    if (isFirstImage)
                    {
                        property.CoverImage = propertyImage.FilePath;
                    }

                    results.Add(new PropertyImageDto
                    {
                        Id = propertyImage.Id,
                        PropertyId = propertyImage.PropertyId,
                        FileName = propertyImage.FileName,
                        OriginalFileName = propertyImage.OriginalFileName,
                        FilePath = propertyImage.FilePath,
                        ThumbnailPath = propertyImage.ThumbnailPath,
                        FileSize = propertyImage.FileSize,
                        ContentType = propertyImage.ContentType,
                        SortOrder = propertyImage.SortOrder,
                        IsMain = propertyImage.IsMain,
                        UploadedAt = propertyImage.UploadedAt,
                        FileUrl = _baseUrl + "/" + propertyImage.FilePath.Replace("\\", "/"),
                        ThumbnailUrl = propertyImage.ThumbnailPath != null ? _baseUrl + "/" + propertyImage.ThumbnailPath.Replace("\\", "/") : null
                    });

                    _logger.LogInformation("Fotoğraf yüklendi: {FileName} -> {PropertyId}", file.FileName, propertyId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fotoğraf yüklenirken hata: {FileName}", file.FileName);
                    throw new BusinessException($"Fotoğraf yüklenirken hata oluştu: {file.FileName}");
                }
            }

            await _context.SaveChangesAsync();
            return results;
        }

        /// <summary>
        /// DeletePropertyImageAsync methodunu çalıştırır.
        /// </summary>
        public async Task<bool> DeletePropertyImageAsync(Guid imageId)
        {
            var image = await _context.PropertyImages.FindAsync(imageId);
            if (image == null)
                throw new NotFoundException("Fotoğraf bulunamadı");

            // Dosyaları sil
            var fullPath = Path.Combine(_baseUploadPath, image.FilePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            if (image.ThumbnailPath != null)
            {
                var thumbPath = Path.Combine(_baseUploadPath, image.ThumbnailPath);
                if (File.Exists(thumbPath))
                    File.Delete(thumbPath);
            }

            // Eğer ana fotoğraf siliniyorsa, başka bir fotoğrafı ana yap
            if (image.IsMain)
            {
                var nextImage = await _context.PropertyImages
                    .Where(img => img.PropertyId == image.PropertyId && img.Id != imageId)
                    .OrderBy(img => img.SortOrder)
                    .FirstOrDefaultAsync();

                if (nextImage != null)
                {
                    nextImage.IsMain = true;
                }

                // Mülkün kapak fotoğrafını güncelle
                var property = await _context.Properties.FindAsync(image.PropertyId);
                if (property != null)
                {
                    property.CoverImage = nextImage?.FilePath;
                }
            }

            _context.PropertyImages.Remove(image);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Fotoğraf silindi: {ImageId}", imageId);

            return true;
        }

        /// <summary>
        /// SetMainImageAsync methodunu çalıştırır.
        /// </summary>
        public async Task<PropertyImageDto> SetMainImageAsync(Guid imageId)
        {
            var image = await _context.PropertyImages.FindAsync(imageId);
            if (image == null)
                throw new NotFoundException("Fotoğraf bulunamadı");

            // Tüm fotoğrafların IsMain'ini false yap
            var allImages = await _context.PropertyImages
                .Where(img => img.PropertyId == image.PropertyId)
                .ToListAsync();

            foreach (var img in allImages)
            {
                img.IsMain = false;
            }

            // Seçili fotoğrafı ana yap
            image.IsMain = true;

            // Mülkün kapak fotoğrafını güncelle
            var property = await _context.Properties.FindAsync(image.PropertyId);
            if (property != null)
            {
                property.CoverImage = image.FilePath;
            }

            await _context.SaveChangesAsync();

            return new PropertyImageDto
            {
                Id = image.Id,
                PropertyId = image.PropertyId,
                FileName = image.FileName,
                OriginalFileName = image.OriginalFileName,
                FilePath = image.FilePath,
                ThumbnailPath = image.ThumbnailPath,
                FileSize = image.FileSize,
                ContentType = image.ContentType,
                SortOrder = image.SortOrder,
                IsMain = image.IsMain,
                UploadedAt = image.UploadedAt,
                FileUrl = _baseUrl + "/" + image.FilePath.Replace("\\", "/"),
                ThumbnailUrl = image.ThumbnailPath != null ? _baseUrl + "/" + image.ThumbnailPath.Replace("\\", "/") : null
            };
        }

        /// <summary>
        /// ReorderImagesAsync methodunu çalıştırır.
        /// </summary>
        public async Task<bool> ReorderImagesAsync(Guid propertyId, List<Guid> imageIds)
        {
            var images = await _context.PropertyImages
                .Where(img => img.PropertyId == propertyId && imageIds.Contains(img.Id))
                .ToListAsync();

            if (images.Count != imageIds.Count)
                throw new BusinessException("Bazı fotoğraflar bulunamadı");

            for (int i = 0; i < imageIds.Count; i++)
            {
                var image = images.First(img => img.Id == imageIds[i]);
                image.SortOrder = i + 1;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Birim Fotoğrafları

        /// <summary>
        /// GetUnitImagesAsync methodunu çalıştırır.
        /// </summary>
        public async Task<List<UnitImageDto>> GetUnitImagesAsync(Guid unitId)
        {
            return await _context.UnitImages
                .Where(img => img.UnitId == unitId)
                .OrderBy(img => img.SortOrder)
                .Select(img => new UnitImageDto
                {
                    Id = img.Id,
                    UnitId = img.UnitId,
                    FileName = img.FileName,
                    OriginalFileName = img.OriginalFileName,
                    FilePath = img.FilePath,
                    ThumbnailPath = img.ThumbnailPath,
                    FileSize = img.FileSize,
                    ContentType = img.ContentType,
                    SortOrder = img.SortOrder,
                    IsMain = img.IsMain,
                    UploadedAt = img.UploadedAt,
                    FileUrl = _baseUrl + "/" + img.FilePath.Replace("\\", "/"),
                    ThumbnailUrl = img.ThumbnailPath != null ? _baseUrl + "/" + img.ThumbnailPath.Replace("\\", "/") : null
                })
                .ToListAsync();
        }

        /// <summary>
        /// UploadUnitImagesAsync methodunu çalıştırır.
        /// </summary>
        public async Task<List<UnitImageDto>> UploadUnitImagesAsync(Guid unitId, List<IFormFile> files)
        {
            var unit = await _context.Units.FindAsync(unitId);
            if (unit == null)
                throw new NotFoundException("Birim bulunamadı");

            if (files == null || files.Count == 0)
                throw new BusinessException("En az bir dosya seçilmelidir");

            var results = new List<UnitImageDto>();
            var currentMaxOrder = await _context.UnitImages
                .Where(img => img.UnitId == unitId)
                .MaxAsync(img => (int?)img.SortOrder) ?? 0;

            var hasMainImage = await _context.UnitImages
                .AnyAsync(img => img.UnitId == unitId && img.IsMain);

            foreach (var file in files)
            {
                if (!IsValidImageFile(file))
                    throw new BusinessException($"Geçersiz dosya formatı: {file.FileName}");

                if (!IsValidImageSize(file))
                    throw new BusinessException($"Dosya boyutu çok büyük: {file.FileName}");

                try
                {
                    currentMaxOrder++;
                    var isFirstImage = !hasMainImage && results.Count == 0 && currentMaxOrder == 1;

                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                    var unitFolder = Path.Combine("units", unitId.ToString());

                    var folderPath = Path.Combine(_baseUploadPath, unitFolder);
                    Directory.CreateDirectory(folderPath);

                    var filePath = Path.Combine(folderPath, uniqueFileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var thumbnailFileName = $"thumb_{uniqueFileName}";
                    var thumbnailPath = Path.Combine(folderPath, thumbnailFileName);
                    
                    try
                    {
                        await CreateThumbnailFromPathAsync(filePath, thumbnailPath, 400, 300);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Thumbnail oluşturulamadı");
                        thumbnailPath = null;
                    }

                    var unitImage = new UnitImage
                    {
                        Id = Guid.NewGuid(),
                        UnitId = unitId,
                        FileName = uniqueFileName,
                        OriginalFileName = file.FileName,
                        FilePath = Path.Combine(unitFolder, uniqueFileName),
                        ThumbnailPath = thumbnailPath != null ? Path.Combine(unitFolder, thumbnailFileName) : null,
                        FileSize = file.Length,
                        ContentType = file.ContentType,
                        SortOrder = currentMaxOrder,
                        IsMain = isFirstImage,
                        UploadedAt = DateTime.UtcNow
                    };

                    _context.UnitImages.Add(unitImage);

                    results.Add(new UnitImageDto
                    {
                        Id = unitImage.Id,
                        UnitId = unitImage.UnitId,
                        FileName = unitImage.FileName,
                        OriginalFileName = unitImage.OriginalFileName,
                        FilePath = unitImage.FilePath,
                        ThumbnailPath = unitImage.ThumbnailPath,
                        FileSize = unitImage.FileSize,
                        ContentType = unitImage.ContentType,
                        SortOrder = unitImage.SortOrder,
                        IsMain = unitImage.IsMain,
                        UploadedAt = unitImage.UploadedAt,
                        FileUrl = _baseUrl + "/" + unitImage.FilePath.Replace("\\", "/"),
                        ThumbnailUrl = unitImage.ThumbnailPath != null ? _baseUrl + "/" + unitImage.ThumbnailPath.Replace("\\", "/") : null
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Birim fotoğrafı yüklenirken hata");
                    throw new BusinessException("Fotoğraf yüklenirken hata oluştu");
                }
            }

            await _context.SaveChangesAsync();
            return results;
        }

        /// <summary>
        /// DeleteUnitImageAsync methodunu çalıştırır.
        /// </summary>
        public async Task<bool> DeleteUnitImageAsync(Guid imageId)
        {
            var image = await _context.UnitImages.FindAsync(imageId);
            if (image == null)
                throw new NotFoundException("Fotoğraf bulunamadı");

            var fullPath = Path.Combine(_baseUploadPath, image.FilePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            if (image.ThumbnailPath != null)
            {
                var thumbPath = Path.Combine(_baseUploadPath, image.ThumbnailPath);
                if (File.Exists(thumbPath))
                    File.Delete(thumbPath);
            }

            _context.UnitImages.Remove(image);
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Genel

        /// <summary>
        /// GetImageBytesAsync methodunu çalıştırır.
        /// </summary>
        public async Task<byte[]> GetImageBytesAsync(string filePath)
        {
            var fullPath = Path.Combine(_baseUploadPath, filePath);
            if (!File.Exists(fullPath))
                throw new NotFoundException("Fotoğraf bulunamadı");

            return await File.ReadAllBytesAsync(fullPath);
        }

        /// <summary>
        /// GetImageUrlAsync methodunu çalıştırır.
        /// </summary>
        public Task<string> GetImageUrlAsync(string filePath, int? width = null, int? height = null)
        {
            if (string.IsNullOrEmpty(filePath))
                return Task.FromResult<string>(null);

            var url = $"{_baseUrl}/{filePath.Replace("\\", "/")}";

            if (width.HasValue || height.HasValue)
            {
                url += $"?w={width}&h={height}";
            }

            return Task.FromResult(url);
        }

        /// <summary>
        /// ImageExistsAsync methodunu çalıştırır.
        /// </summary>
        public Task<bool> ImageExistsAsync(string filePath)
        {
            var fullPath = Path.Combine(_baseUploadPath, filePath);
            return Task.FromResult(File.Exists(fullPath));
        }

        /// <summary>
        /// GetTotalStorageSizeAsync methodunu çalıştırır.
        /// </summary>
        public async Task<long> GetTotalStorageSizeAsync(Guid tenantId)
        {
            var propertyImages = await _context.PropertyImages
                .Where(img => img.Property.TenantId == tenantId)
                .SumAsync(img => img.FileSize);

            var unitImages = await _context.UnitImages
                .Where(img => img.Unit.Property.TenantId == tenantId)
                .SumAsync(img => img.FileSize);

            return propertyImages + unitImages;
        }

        #endregion

        #region Optimizasyon

        /// <summary>
        /// OptimizeImageAsync methodunu çalıştırır.
        /// </summary>
        public async Task<string> OptimizeImageAsync(string filePath, int maxWidth = 1920, int maxHeight = 1080, int quality = 85)
        {
            var fullPath = Path.Combine(_baseUploadPath, filePath);
            if (!File.Exists(fullPath))
                throw new NotFoundException("Fotoğraf bulunamadı");

            try
            {
                using var image = await Image.LoadAsync(fullPath);
                
                // Boyutlandırma
                if (image.Width > maxWidth || image.Height > maxHeight)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(maxWidth, maxHeight),
                        Mode = ResizeMode.Max
                    }));
                }

                // Optimize edilmiş dosyayı kaydet
                var optimizedFileName = $"opt_{Path.GetFileName(filePath)}";
                var optimizedPath = Path.Combine(Path.GetDirectoryName(fullPath), optimizedFileName);
                
                await image.SaveAsync(optimizedPath, new JpegEncoder
                {
                    Quality = quality
                });

                // Orijinal dosyayı optimize edilmiş dosya ile değiştir
                File.Delete(fullPath);
                File.Move(optimizedPath, fullPath);

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fotoğraf optimize edilirken hata: {FilePath}", filePath);
                return filePath; // Orijinali döndür
            }
        }

        /// <summary>
        /// CreateThumbnailAsync methodunu çalıştırır.
        /// </summary>
        public async Task<string> CreateThumbnailAsync(string filePath, int width = 300, int height = 200)
        {
            var fullPath = Path.Combine(_baseUploadPath, filePath);
            if (!File.Exists(fullPath))
                throw new NotFoundException("Fotoğraf bulunamadı");

            var thumbnailFileName = $"thumb_{Path.GetFileName(filePath)}";
            var thumbnailPath = Path.Combine(Path.GetDirectoryName(fullPath), thumbnailFileName);

            await CreateThumbnailFromPathAsync(fullPath, thumbnailPath, width, height);

            var relativePath = Path.Combine(Path.GetDirectoryName(filePath), thumbnailFileName);
            return relativePath;
        }

        /// <summary>
        /// CreateThumbnailFromPathAsync methodunu çalıştırır.
        /// </summary>
        private async Task CreateThumbnailFromPathAsync(string sourcePath, string destinationPath, int width, int height)
        {
            using var image = await Image.LoadAsync(sourcePath);
            
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Crop
            }));

            await image.SaveAsync(destinationPath, new JpegEncoder
            {
                Quality = 80
            });
        }

        #endregion

        #region Validasyon

        /// <summary>
        /// IsValidImageFile methodunu çalıştırır.
        /// </summary>
        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLower();
            return _allowedExtensions.Contains(extension) && _allowedMimeTypes.Contains(file.ContentType.ToLower());
        }

        /// <summary>
        /// IsValidImageSize methodunu çalıştırır.
        /// </summary>
        public bool IsValidImageSize(IFormFile file, long maxSizeInBytes = 5 * 1024 * 1024)
        {
            return file.Length <= MaxFileSize && file.Length <= maxSizeInBytes;
        }

        /// <summary>
        /// GetAllowedExtensions methodunu çalıştırır.
        /// </summary>
        public List<string> GetAllowedExtensions()
        {
            return _allowedExtensions.ToList();
        }

        #endregion

        #region Yardımcı Metotlar

        /// <summary>
        /// EnsureDirectoriesExist methodunu çalıştırır.
        /// </summary>
        private void EnsureDirectoriesExist()
        {
            var directories = new[]
            {
                _baseUploadPath,
                Path.Combine(_baseUploadPath, "properties"),
                Path.Combine(_baseUploadPath, "units"),
                Path.Combine(_baseUploadPath, "temp")
            };

            foreach (var dir in directories)
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
        }

        #endregion
    }
}