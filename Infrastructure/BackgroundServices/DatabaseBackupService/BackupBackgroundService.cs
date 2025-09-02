using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WTelegram;

namespace Infrastructure.BackgroundServices.DatabaseBackupService
{

    public class BackupBackgroundService : BackgroundService
    {
        private readonly Bot _bot;
        private readonly ILogger<BackupBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly BackupSettings _settings;

        public BackupBackgroundService(Bot bot,
            IOptions<BackupSettings> settings,
            ILogger<BackupBackgroundService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.IsEnabled)
            {
                _logger.LogInformation("Backup service is disabled. Set 'IsEnabled' to true to enable backups.");
                return;
            }

            if (_settings.BackupPeriod <= 0)
            {
                _logger.LogWarning(
                    "BackupPeriod must be > 0. Current value: {BackupPeriod}. Backup service will not start.",
                    _settings.BackupPeriod);
                return;
            }
            
            try
            {
                var me = await _bot.GetMe();
                _logger.LogInformation("Backup bot authenticated as {username} (id: {id})", me.Username, me.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to authenticate via telegram bot in backup service. Service cannot start without a working bot.");
                throw new Exception("backup service failed to authenticate bot", ex);
            }

            _logger.LogInformation("Starting backup loop. Interval: {minutes} minute(s).", _settings.BackupPeriod);
            
            var interval = TimeSpan.FromMinutes(_settings.BackupPeriod);
            
            await RunBackupOnceSafeAsync(stoppingToken);

            using var timer = new PeriodicTimer(interval);
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await RunBackupOnceSafeAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Backup loop canceled (service stopping).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in backup loop. Service will stop.");
                throw;
            }
        }


        private async Task RunBackupOnceSafeAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var backupJob = scope.ServiceProvider.GetRequiredService<BackupJob>();

                _logger.LogInformation("Starting backup job at {timeUtc}", DateTime.UtcNow);
                await backupJob.StartBackup(cancellationToken);
                _logger.LogInformation("Backup job finished at {timeUtc}", DateTime.UtcNow);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Backup job canceled (shutdown requested).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Backup job failed. See exception for details.");
            }
        }
    }
}