using Application.Core;
using Application.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.EmailService;
using Infrastructure.EmailService.Models;
using Infrastructure.GeminiWrapper;
using Infrastructure.GeminiWrapper.Models;
using Infrastructure.OmdbWrapper;
using Infrastructure.OmdbWrapper.Models;
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

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Application.Users.Commands.Update.Handler).Assembly));
            services.AddAutoMapper(typeof(MappingProfiles).Assembly);
            services.AddHttpContextAccessor();
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<Application.Users.Commands.Update.Validator>();
           
            services.AddScoped<IUserAccessor, UserAccessor>();
            services.AddScoped<IGeminiService, GeminiService>();
            services.AddScoped<IOmdbService, OMDbService>();

            services.Configure<SmtpSettings>(config.GetSection("SmtpSettings"));
            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
            services.Configure<OMDbSettings>(config.GetSection("OMDbSettings"));
            services.Configure<GeminiSettings>(config.GetSection("GeminiSettings"));

            services.AddTransient<IEmailService, EmailService>();
            return services;
        }
    }
}