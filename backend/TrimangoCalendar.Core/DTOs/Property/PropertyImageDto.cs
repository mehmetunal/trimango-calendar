// backend/TrimangoCalendar.Core/DTOs/ImageDtos.cs
namespace TrimangoCalendar.Core.DTOs.Property
{
    public class PropertyImageDto
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string FilePath { get; set; }
        public string ThumbnailPath { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public int SortOrder { get; set; }
        public bool IsMain { get; set; }
        public DateTime UploadedAt { get; set; }
        public string FileUrl { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public class UnitImageDto
    {
        public Guid Id { get; set; }
        public Guid UnitId { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string FilePath { get; set; }
        public string ThumbnailPath { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public int SortOrder { get; set; }
        public bool IsMain { get; set; }
        public DateTime UploadedAt { get; set; }
        public string FileUrl { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public class ImageUploadResult
    {
        public bool Success { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ErrorMessage { get; set; }
    }
}