public class PropertyImage
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
    
    public Property Property { get; set; }
}

