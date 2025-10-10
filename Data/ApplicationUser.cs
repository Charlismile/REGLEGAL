using Microsoft.AspNetCore.Identity;

namespace REGISTROLEGAL.Data;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; } = String.Empty;
    public string? LastName { get; set; } = String.Empty;
    public DateTime? CreatedOn { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public bool IsAproved { get; set; } = true;
    public bool IsFromActiveDirectory { get; set; } = true;
    public bool MustChangePassword { get; set; }
}