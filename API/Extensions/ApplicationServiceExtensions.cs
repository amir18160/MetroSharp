using Hangfire;
using Hangfire.SQLite;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using FluentValidation;
using FluentValidation.AspNetCore;

using Application.Core;
using Application.Interfaces;

using Persistence;

using Infrastructure.BackgroundServices.Models;
using Infrastructure.BackgroundServices.TelegramBot;
using Infrastructure.BackgroundServices.TorrentProcessTask;
using Infrastructure.EmailService;
using Infrastructure.EmailService.Models;
using Infrastructure.GeminiWrapper;
using Infrastructure.GeminiWrapper.Models;
using Infrastructure.OmdbWrapper;
using Infrastructure.OmdbWrapper.Models;
using Infrastructure.ProwlarrWrapper.Models;
using Infrastructure.QbitTorrentClient;
using Infrastructure.QbitTorrentClient.Models;
using Infrastructure.Scrapers;
using Infrastructure.Security;
using Infrastructure.Utilities;

using Telegram.Bot.Polling;

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

            var ConnectionString = config.GetConnectionString("DownloadContext");

            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSQLiteStorage(ConnectionString));

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = 1;
            });

            services.AddTransient<TorrentTaskProcessor>();
            services.AddTransient<TaskCleaner>();
            services.AddHostedService<TaskPollingService>();

            services.AddSingleton<WTelegram.Bot>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<TelegramBotSettings>>().Value;
                var isDevelopment = sp.GetRequiredService<IHostEnvironment>().IsDevelopment();
                var dbConnection = new SqliteConnection(settings.TelegramContext);
                var botToken = isDevelopment ? settings.DevelopmentBotToken : settings.BotToken;
                return new WTelegram.Bot(botToken, settings.AppId, settings.ApiHash, dbConnection);
            });

            services.AddSingleton<IUpdateHandler, TelegramUpdateHandler>();
            services.AddHostedService<TelegramBot>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Application.Users.Commands.Update.Handler).Assembly));
            services.AddAutoMapper(typeof(MappingProfiles).Assembly, typeof(Infrastructure.Core.MappingProfiles).Assembly);
            services.AddHttpContextAccessor();
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<Application.Users.Commands.Update.Validator>();

            services.AddScoped<IUserAccessor, UserAccessor>();
            services.AddScoped<IGeminiService, GeminiService>();
            services.AddScoped<IScraperFacade, ScraperFacade>();
            services.AddScoped<IUtilitiesFacade, UtilitiesFacade>();

            services.AddHttpClient<IOmdbService, OMDbService>();
            services.AddHttpClient<ZipDownloader>();

            services.AddSingleton<IQbitClient, QbitClient>();
            services.AddTransient<IEmailService, EmailService>();


            services.Configure<SmtpSettings>(config.GetSection("SmtpSettings"));
            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
            services.Configure<OMDbSettings>(config.GetSection("OMDbSettings"));
            services.Configure<GeminiSettings>(config.GetSection("GeminiSettings"));
            services.Configure<QbitTorrentSettings>(config.GetSection("QbitTorrentSettings"));
            services.Configure<TorrentTaskSettings>(config.GetSection("TorrentTaskSettings"));
            services.Configure<TelegramBotSettings>(config.GetSection("TelegramBotSettings"));
            services.Configure<ProwlarrSettings>(config.GetSection("ProwlarrSettings"));


            services.AddScoped<TorrentTaskStartConditionChecker>();
            services.AddScoped<CheckAndGenerateOMDbData>();
            services.AddScoped<StartTorrentTaskDownload>();
            services.AddScoped<DownloadSubtitleForTorrent>();
            services.AddScoped<PairSubtitleWithVideos>();
            services.AddScoped<SubtitleEditor>();
            services.AddScoped<FFmpegTaskProcessor>();
            services.AddScoped<FileUploadProcessor>();
            services.AddScoped<TaskCleaner>();

            return services;
        }

        public static IServiceCollection AddCorsPolicy(
            this IServiceCollection services,
            string policyName,
            IConfiguration config,
            IWebHostEnvironment env)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: policyName, policy =>
                {
                    if (env.IsDevelopment())
                    {
                        policy.WithOrigins("http://localhost", "null")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    }
                    else
                    {
                        var allowedOrigins = config.GetValue<string>("CorsSettings:AllowedOrigins");
                        if (!string.IsNullOrEmpty(allowedOrigins))
                        {
                            policy.WithOrigins(allowedOrigins.Split(';', StringSplitOptions.RemoveEmptyEntries))
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                        }
                    }
                });
            });

            return services;
        }
    }
}