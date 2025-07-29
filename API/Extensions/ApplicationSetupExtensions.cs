using Domain.Entities;
using Infrastructure.BackgroundServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Seeds;

namespace API.Extensions
{
    public static class ApplicationSetupExtensions
    {
        public static async Task ConfigureDatabaseAndSeedDataAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var dataContext = services.GetRequiredService<DataContext>();
                    var downloadContext = services.GetRequiredService<DownloadContext>();
                    var userManager = services.GetRequiredService<UserManager<User>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var logger = services.GetRequiredService<ILogger<Program>>();

                    logger.LogInformation("Running Application Startup...");

                    var DlContextPendingMigrations = await downloadContext.Database.GetPendingMigrationsAsync();
                    if (DlContextPendingMigrations.Any())
                    {
                        logger.LogInformation("Applying pending migrations for Download Context...");
                        await downloadContext.Database.MigrateAsync();
                    }
                    else
                    {
                        logger.LogInformation("No pending migrations  for Download Context.");
                    }

                    var DataContextPendingMigrations = await dataContext.Database.GetPendingMigrationsAsync();
                    if (DataContextPendingMigrations.Any())
                    {
                        logger.LogInformation("Applying pending migrations for Data Context...");
                        await dataContext.Database.MigrateAsync();
                    }
                    else
                    {
                        logger.LogInformation("No pending migrations for Data Context.");
                    }

                    var shouldSeedRoles = !roleManager.Roles.Any();
                    if (shouldSeedRoles)
                    {
                        logger.LogInformation("Seeding roles...");
                        await SeedRoles.SeedUserRoles(services);
                    }

                    var shouldSeedData = !(await dataContext.Users.AnyAsync());
                    if (shouldSeedData)
                    {
                        logger.LogInformation("Seeding application data...");
                        await DataSeeder.SeedAsync(dataContext, userManager);
                    }

                    await TaskRecovery.RecoverInterruptedTasks(services);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred during migration/seeding.");
                }
            }
        }
    }
}