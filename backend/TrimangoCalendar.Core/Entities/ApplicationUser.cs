using Microsoft.AspNetCore.Identity;

namespace TrimangoCalendar.Core.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? TenantId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
