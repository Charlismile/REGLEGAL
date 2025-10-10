namespace REGISTROLEGAL.Models;

public class ActiveDirectoryUserModel
{
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string MiddleName { get; set; } = "";
    public bool Enabled { get; set; } = false;
    public DateTime? LastLoginDate { get; set; }
}