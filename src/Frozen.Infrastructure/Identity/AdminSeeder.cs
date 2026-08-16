using Frozen.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Frozen.Infrastructure.Identity;

public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var settings = services.GetRequiredService<IOptions<SeedAdminSettings>>().Value;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(AdminSeeder));

        if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
            await roleManager.CreateAsync(new IdentityRole<Guid>(UserRoles.Admin));

        var existingAdmins = await userManager.GetUsersInRoleAsync(UserRoles.Admin);
        if (existingAdmins.Count > 0)
            return;

        var user = await userManager.FindByEmailAsync(settings.Email);
        if (user is not null)
        {
            await userManager.AddToRoleAsync(user, UserRoles.Admin);
            logger.LogWarning("Promoted existing user {Email} to Admin as part of startup seeding.", settings.Email);
            return;
        }

        user = new ApplicationUser
        {
            UserName = settings.Email,
            Email = settings.Email,
            FirstName = settings.FirstName,
            LastName = settings.LastName
        };

        var result = await userManager.CreateAsync(user, settings.Password);
        if (!result.Succeeded)
        {
            logger.LogError(
                "Failed to seed default admin user {Email}: {Errors}",
                settings.Email,
                string.Join(" ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, UserRoles.Admin);
        logger.LogWarning(
            "Seeded default admin user {Email}. Change its password after first login.",
            settings.Email);
    }
}
