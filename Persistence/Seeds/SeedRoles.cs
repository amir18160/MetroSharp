using Domain.Core;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Persistence.Seeds;

public class SeedRoles
{
    public static async Task SeedUserRoles(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var logger = serviceProvider.GetRequiredService<ILogger<SeedRoles>>();

        var roles = new[] { "Admin", "Owner", "User" };

        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // add admin user
        var defaultPassword = "Pass1234";
        var defaultUser = new User
        {
            UserName = "admin",
            Email = "admin@gmail.com",
            Name = "admin",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(defaultUser, "Pass1234");
        await userManager.AddToRoleAsync(defaultUser, Roles.Owner);

        logger.LogInformation("Owner user is initialized. Username: {username}, Password: {password}.",
            defaultUser.UserName, defaultPassword);
        logger.LogInformation("If you loose Owner details there is no safe and easy way to recover it.");
    }
}