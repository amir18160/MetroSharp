using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using Application.Interfaces;
using Infrastructure.EmailService.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly string _templateDirectory;
        private readonly string _appName;
        private readonly ILogger<EmailService> _logger;
        private readonly IHostEnvironment _env;

        public EmailService(IOptions<SmtpSettings> options, IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger, IHostEnvironment env)
        {
            _logger = logger;
            _settings = options.Value;
            _templateDirectory = emailSettings.Value.TemplatePath;
            _appName = emailSettings.Value.AppName;
            _env = env;
        }

        private SmtpClient CreateClient()
        {
            return new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };
        }

        private async Task<string> ReadAllTextAsync(TemplatesEnum template)
        {
            var path = template switch
            {
                TemplatesEnum.Confirmation => Path.Combine(_templateDirectory, "confirmation.html"),
                TemplatesEnum.PasswordReset => Path.Combine(_templateDirectory, "reset-password.html"),
                _ => throw new ArgumentOutOfRangeException(nameof(template))
            };

            return await File.ReadAllTextAsync(path, Encoding.UTF8).ConfigureAwait(false);
        }

        private string RenderTemplate(string templateContent, Dictionary<string, string> values)
        {
            return Regex.Replace(templateContent, "{{(.*?)}}", match =>
            {
                var key = match.Groups[1].Value.Trim();
                return values.ContainsKey(key) ? values[key] : match.Value;
            });
        }

        private async Task SendEmailAsync(string to, string subject, string bodyHtml)
        {
            if (string.IsNullOrWhiteSpace(to) || !MailAddress.TryCreate(to, out _))
            {
                throw new Exception($"Emails is Invalid {to}");
            }

            using var client = CreateClient();
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.From),
                Subject = subject,
                Body = bodyHtml,
                IsBodyHtml = true
            };

            message.To.Add(to);
            await client.SendMailAsync(message);
        }

        public async Task<bool> SendConfirmationEmail(string to, string userName, string confirmationLink)
        {
            try
            {
                var template = await ReadAllTextAsync(TemplatesEnum.Confirmation);

                if (_env.IsDevelopment())
                {
                    confirmationLink = "http://localhost:9992/confirmation-code/" + confirmationLink;
                }

                var body = RenderTemplate(template, new Dictionary<string, string>
                {
                    ["fullName"] = userName,
                    ["confirmationLink"] = confirmationLink,
                    ["year"] = DateTime.UtcNow.Year.ToString(),
                    ["appName"] = _appName
                });

                await SendEmailAsync(to, "تأیید حساب کاربری", body).ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending confirmation email.");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmail(string to, string userName, string resetLink)
        {
            try
            {
                var template = await ReadAllTextAsync(TemplatesEnum.PasswordReset);

                if (_env.IsDevelopment())
                {
                    resetLink = "http://localhost:9992/password-reset/" + resetLink;
                }

                var body = RenderTemplate(template, new Dictionary<string, string>
                {
                    ["fullName"] = userName,
                    ["resetLink"] = resetLink,
                    ["year"] = DateTime.UtcNow.Year.ToString(),
                    ["appName"] = _appName
                });

                await SendEmailAsync(to, "بازیابی رمز عبور", body).ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending confirmation email.");
                return false;
            }
        }
    }
}
