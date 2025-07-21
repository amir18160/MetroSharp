using Application.Core;
using Application.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SQLite;
using Infrastructure.BackgroundServices.TorrentProcessTask;
using Infrastructure.EmailService;
using Infrastructure.EmailService.Models;
using Infrastructure.GeminiWrapper;
using Infrastructure.GeminiWrapper.Models;
using Infrastructure.OmdbWrapper;
using Infrastructure.OmdbWrapper.Models;
using Infrastructure.QbitTorrentClient;
using Infrastructure.QbitTorrentClient.Models;
using Infrastructure.Scrapers;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddOpenApi();

            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseSqlite(config.GetConnectionString("DataContext"));
            });

            services.AddDbContext<DownloadContext>(opt =>
            {
                opt.UseSqlite(config.GetConnectionString("DownloadContext"));
            });



            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSQLiteStorage(config.GetConnectionString("DownloadContext")));

            
            
            services.AddHangfireServer(options =>
            {
                options.WorkerCount = 5;
            });

            services.AddScoped<TorrentTaskProcessor>();
            services.AddHostedService<TaskPollingService>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Application.Users.Commands.Update.Handler).Assembly));
            services.AddAutoMapper(typeof(MappingProfiles).Assembly, typeof(Infrastructure.Core.MappingProfiles).Assembly);
            services.AddHttpContextAccessor();
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<Application.Users.Commands.Update.Validator>();

            services.AddScoped<IUserAccessor, UserAccessor>();
            services.AddScoped<IGeminiService, GeminiService>();
            services.AddScoped<IScraperFacade, ScraperFacade>();

            services.AddHttpClient<IOmdbService, OMDbService>();

            services.Configure<SmtpSettings>(config.GetSection("SmtpSettings"));
            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
            services.Configure<OMDbSettings>(config.GetSection("OMDbSettings"));
            services.Configure<GeminiSettings>(config.GetSection("GeminiSettings"));
            services.Configure<QbitTorrentSettings>(config.GetSection("QbitTorrentSettings"));

            services.AddSingleton<IQbitClient, QbitClient>();

            services.AddTransient<IEmailService, EmailService>();
            return services;
        }
    }
}