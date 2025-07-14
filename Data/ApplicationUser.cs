using Microsoft.AspNetCore.Identity;

namespace REGISTROLEGAL.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string? Nombre { get; set; } = String.Empty;
    public string? Apellido { get; set; } = String.Empty;
    public DateTime? CreadoEn { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public bool IsAproved { get; set; } = true;
    public bool IsFromActiveDirectory { get; set; } = true;
}