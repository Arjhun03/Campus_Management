using Campus_Management.Models;
using Microsoft.AspNetCore.Identity;

namespace Campus_Management.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider)
    {
        var userManager =
            serviceProvider.GetRequiredService<
                UserManager<ApplicationUser>>();

        var roleManager =
            serviceProvider.GetRequiredService<
                RoleManager<IdentityRole>>();

        const string adminRole = "Admin";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(
                new IdentityRole(adminRole));
        }

        var adminEmail =
            serviceProvider
                .GetRequiredService<IConfiguration>()
                ["SeedUsers:AdminEmail"];

        var adminPassword =
            serviceProvider
                .GetRequiredService<IConfiguration>()
                ["SeedUsers:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var existingAdmin =
            await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin != null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Administrator",
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(
            admin,
            adminPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                ", ",
                result.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Failed to create admin user: {errors}");
        }

        await userManager.AddToRoleAsync(
            admin,
            adminRole);
    }
}