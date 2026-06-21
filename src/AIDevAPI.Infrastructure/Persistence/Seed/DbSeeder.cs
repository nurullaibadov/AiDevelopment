using AIDevAPI.Domain.Constants;
using AIDevAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AIDevAPI.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger logger)
    {
        foreach (var roleName in RoleConstants.AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName, $"{roleName} rolu"));
                logger.LogInformation("Rol yaradıldı: {Role}", roleName);
            }
        }

        const string adminEmail = "admin@aidev.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FirstName = "Sistem",
                LastName = "Administratoru",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@12345");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, RoleConstants.Admin);
                logger.LogInformation("Admin istifadəçisi yaradıldı: {Email} / Şifrə: Admin@12345", adminEmail);
            }
            else
            {
                logger.LogError("Admin istifadəçisi yaradıla bilmədi: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
