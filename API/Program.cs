using API.Extensions;
using API.Middleware;
using Domain.Core;
using Hangfire;
using Serilog;


Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();

        if (context.HostingEnvironment.IsDevelopment())
        {
            configuration.WriteTo.Console();
        }
    });

    builder.Services.AddControllers();
    builder.Services.AddSwagger();
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddIdentityServices(builder.Configuration);
    builder.Services.AddCorsPolicy(CorsPolicy.AllowOrigins, builder.Configuration, builder.Environment);
    builder.Services.AddLocalization(builder.Configuration, builder.Environment);
    builder.Services.AddTelegramService(builder.Configuration);

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.UseMiddleware<ExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseHangfireDashboard();
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Metro API V1");
            c.EnablePersistAuthorization();
        });
    }

    app.UseCors(CorsPolicy.AllowOrigins);


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
