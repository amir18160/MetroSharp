using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FluentValidation;
using FluentValidation.AspNetCore;
using Application.Core;
using Application.Interfaces;
using Persistence;
using Infrastructure.BackgroundServices.TelegramBot.Configs;
using Infrastructure.BackgroundServices.TelegramBot;
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
using Infrastructure.Utilities;
using Infrastructure.SystemInfo;
using Infrastructure.ProwlarrWrapper.Models;
using Infrastructure.ProwlarrWrapper;
using Infrastructure.TMDbService.Models;
using Infrastructure.TMDbService;
using Infrastructure.FileStorageService.Models;
using Infrastructure.FileStorageService;
using System.Text.Json;
using Domain.Core;
using Domain.Entities;
using Domain.Models.TelegramBot.Messages;
using DotNetEd.CoreAdmin;
using Infrastructure.BackgroundServices.DatabaseBackupService;
using Infrastructure.BackgroundServices.SitemapGeneratorService;
using Infrastructure.BackgroundServices.TelegramBot.Admin;
using Infrastructure.BackgroundServices.TelegramBot.Command;
using Infrastructure.BackgroundServices.TelegramBot.Keyboard;
using Infrastructure.BackgroundServices.TelegramBot.InlineQuery;
using Infrastructure.BackgroundServices.TelegramBot.CallbackQuery;
using Infrastructure.BackgroundServices.TelegramBot.Messages;
using Infrastructure.BackgroundServices.TelegramBot.Middlewares;
using Infrastructure.DocumentUsageService;
using Infrastructure.DocumentUsageService.Models;
using Infrastructure.ImageEditorService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using Quartz;


namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddOpenApi("internal");

        services.AddDbContext<DataContext>(opt => { opt.UseSqlite(config.GetConnectionString("DataContext")); });

        services.AddRazorPages();
        services.AddServerSideBlazor();

        services.AddDbContext<DownloadContext>(opt =>
        {
            opt.UseSqlite(config.GetConnectionString("DownloadContext"));
        });
        services.AddDbContextFactory<DownloadContext>(
            options => { options.UseSqlite(config.GetConnectionString("DownloadContext")); },
            ServiceLifetime.Transient);


        services.AddQuartz(q =>
        {
            var maxConcurrency = config.GetValue("TorrentTaskSettings:MaxConcurrency", 10);
            q.SchedulerName = "MainQuartzScheduler";
            q.UseDefaultThreadPool(tp => { tp.MaxConcurrency = maxConcurrency; });
        });
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        services.AddHostedService<TaskPollingService>();
        services.AddScoped<TorrentTaskJob>();
        services.AddScoped<TaskCleanerJob>();
        services.AddScoped<TorrentTaskProcessor>();
        services.AddScoped<TaskCleaner>();
        services.AddScoped<TorrentTaskStartConditionChecker>();
        services.AddScoped<CheckAndGenerateOMDbData>();
        services.AddScoped<StartTorrentTaskDownload>();
        services.AddScoped<ExtractSubtitleForTorrent>();
        services.AddScoped<PairSubtitleWithVideos>();
        services.AddScoped<SubtitleEditor>();
        services.AddScoped<FFmpegTaskProcessor>();
        services.AddScoped<FileUploadProcessor>();
        services.AddScoped<TaskCleaner>();
        services.AddScoped<ITaskManager, TaskManager>();

        services.AddHostedService<SitemapGeneratorService>();

        services.AddHostedService<BackupBackgroundService>();
        services.AddScoped<BackupJob>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Application.Users.Commands.Update.Handler).Assembly));
        services.AddAutoMapper(typeof(MappingProfiles).Assembly, typeof(Infrastructure.Core.MappingProfiles).Assembly);
        services.AddHttpContextAccessor();
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<Application.Users.Commands.Update.Validator>();

        services.AddScoped<IUserAccessor, UserAccessor>();
        services.AddScoped<IGeminiService, GeminiService>();
        services.AddScoped<IScraperFacade, ScraperFacade>();
        services.AddScoped<IUtilitiesFacade, UtilitiesFacade>();
        services.AddScoped<ISystemInfoService, SystemInfoService>();
        services.AddScoped<IProwlarr, Prowlarr>();
        services.AddScoped<ITMDbService, TMDbService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<ITelegramManuelMessages, ManuelMessages>();
        services.AddScoped<IDocumentUsageService, DocumentUsageService>();

        services.AddScoped<ImageEditorService>();

        services.AddSingleton<IQbitClient, QbitClient>();
        services.AddTransient<IEmailService, EmailService>();

        services.AddHttpClient<IOmdbService, OMDbService>();
        services.AddHttpClient<ZipDownloader>();

        services.Configure<SmtpSettings>(config.GetSection("SmtpSettings"));
        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        services.Configure<OMDbSettings>(config.GetSection("OMDbSettings"));
        services.Configure<GeminiSettings>(config.GetSection("GeminiSettings"));
        services.Configure<QbitTorrentSettings>(config.GetSection("QbitTorrentSettings"));
        services.Configure<TorrentTaskSettings>(config.GetSection("TorrentTaskSettings"));
        services.Configure<TelegramBotSettings>(config.GetSection("TelegramBotSettings"));
        services.Configure<ProwlarrSettings>(config.GetSection("ProwlarrSettings"));
        services.Configure<TMDbSettings>(config.GetSection("TMDbSettings"));
        services.Configure<FileStorageSettings>(config.GetSection("FileStorageSettings"));
        services.Configure<BackupSettings>(config.GetSection("BackupSettings"));
        services.Configure<SitemapSettings>(config.GetSection("SitemapSettings"));
        services.Configure<ImageEditorSettings>(config.GetSection("ImageEditorSettings"));
        services.Configure<FreePlanSettings>(config.GetSection("FreePlanSettings"));

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
            options.AddPolicy(policyName, policy =>
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
                        policy.WithOrigins(allowedOrigins.Split(';', StringSplitOptions.RemoveEmptyEntries))
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddLocalization(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        var basePath = AppContext.BaseDirectory;
        var path = Path.Combine(basePath, "Localization/BotMessages.json");
        var botMessagesJson = File.ReadAllText(path);

        var serializerOption = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var botMessages = JsonSerializer.Deserialize<BotMessages>(botMessagesJson, serializerOption);

        services.AddSingleton(botMessages);

        return services;
    }

    public static IServiceCollection AddTelegramService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<TelegramBotSettings>>().Value;
            var isDevelopment = sp.GetRequiredService<IHostEnvironment>().IsDevelopment();
            var connectionString = configuration.GetConnectionString("TelegramContext");
            var dbConnection = new SqliteConnection(connectionString);
            var botToken = isDevelopment ? settings.DevelopmentBotToken : settings.BotToken;
            return new WTelegram.Bot(botToken, settings.AppId, settings.ApiHash, dbConnection)
            {
                WantUnknownTLUpdates = true
            };
        });

        services.AddHostedService<TelegramBot>();

        services.AddScoped<CommandHandler>();
        services.AddScoped<StartCommandHandler>();

        services.AddScoped<KeyboardHandler>();
        services.AddScoped<DefaultKeyboards>();

        services.AddScoped<InlineQueryButtons>();
        services.AddScoped<InlineQueryHandler>();
        services.AddScoped<ChosenInlineHandler>();
        services.AddScoped<HandleIMDbId>();
        services.AddScoped<HandleSearchTitlesInline>();

        services.AddScoped<CallbackQueryButtons>();
        services.AddScoped<CallbackTitleListHandler>();
        services.AddScoped<CallbackDownloadHandler>();
        services.AddScoped<CallbackHandler>();

        services.AddScoped<IsAdmin>();
        services.AddScoped<HandleForwardedFiles>();

        services.AddScoped<JoinCheck>();

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token in this format: Bearer {your token}"
            });


            c.CustomSchemaIds(type => type.FullName);
        });
        return services;
    }
    
}