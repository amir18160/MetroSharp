using API.Extensions;
using API.Middleware;
using Domain.Entities;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Seeds;

var builder = WebApplication.CreateBuilder(args);




// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHangfireDashboard();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during migration/seeding.");
    }
}



app.Run();
