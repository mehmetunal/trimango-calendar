namespace TrimangoCalendar.Core.Entities;

public class Report
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? AgencyId { get; set; }
    public string Name { get; set; }
    public ReportType Type { get; set; }
    public ReportPeriod Period { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Filters { get; set; } // JSON filtreler
    
    public string FilePath { get; set; } // Excel/PDF dosya yolu
    public string FileFormat { get; set; } // "Excel", "PDF"
    public long FileSize { get; set; }
    
    public ReportStatus Status { get; set; }
    public string ErrorMessage { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string CreatedBy { get; set; }
    
    public Tenant Tenant { get; set; }
}

