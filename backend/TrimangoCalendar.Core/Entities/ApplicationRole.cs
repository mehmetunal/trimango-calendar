using Microsoft.AspNetCore.Identity;

namespace TrimangoCalendar.Core.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
}
