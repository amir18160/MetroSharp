using API.Extensions;
using API.Middleware;
using Domain.Core;
using Hangfire;
using Serilog;

// Configure Serilog for bootstrap logging first
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog, now reading from appsettings.json
    builder.Host.UseSerilog((context, services, configuration) => {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();
        
        // Add the console sink only in the Development environment
        if (context.HostingEnvironment.IsDevelopment())
        {
            configuration.WriteTo.Console();
        }
    });

    builder.Services.AddControllers();
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddIdentityServices(builder.Configuration);
    builder.Services.AddCorsPolicy(CorsPolicy.AllowOrigins, builder.Configuration, builder.Environment);

    var app = builder.Build();

    // Use Serilog for HTTP request logging
    app.UseSerilogRequestLogging();

    app.UseMiddleware<ExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseCors(CorsPolicy.AllowOrigins);

    app.UseHangfireDashboard();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    await app.ConfigureDatabaseAndSeedDataAsync();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occurred during bootstrapping");
}
finally
{
    Log.CloseAndFlush();
}
