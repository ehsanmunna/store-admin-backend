namespace Frozen.Infrastructure.Identity;

public class SeedAdminSettings
{
    public const string SectionName = "SeedAdmin";

    public string Email { get; set; } = "admin@frozen.local";
    public string Password { get; set; } = "ChangeMe123!";
    public string FirstName { get; set; } = "Super";
    public string LastName { get; set; } = "Admin";
}
