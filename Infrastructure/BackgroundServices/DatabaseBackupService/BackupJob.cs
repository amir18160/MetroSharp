using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WTelegram;

namespace Infrastructure.BackgroundServices.DatabaseBackupService
{
    public class BackupJob
    {
        private readonly Bot _bot;
        private readonly BackupSettings  _backupSettings;
        private readonly ILogger<BackupJob> _logger;

        private const int DefaultMaxAttempts = 3;
        private const int DefaultBaseDelayMilliseconds = 1000;

        public BackupJob(WTelegram.Bot bot, IOptions<BackupSettings> backupSettings, ILogger<BackupJob> logger)
        {
            _bot = bot;
            _backupSettings = backupSettings.Value;
            _logger = logger;
        }

        public async Task StartBackup(CancellationToken cancellationToken = default)
        {
            if (_backupSettings.BackupPaths == null || _backupSettings.BackupPaths.Count == 0)
            {
                _logger.LogWarning("No backup paths configured. Skipping backup run.");
                return;
            }

            foreach (var path in _backupSettings.BackupPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(path))
                {
                    _logger.LogWarning("Encountered empty backup path in configuration. Skipping entry.");
                    continue;
                }

                try
                {
                    if (!File.Exists(path))
                    {
                        _logger.LogWarning("Backup file does not exist: {path}", path);
                        continue;
                    }

                    var fileName = Path.GetFileName(path);
                    _logger.LogInformation("Beginning backup for file: {fileName} (full path: {path})", fileName, path);

                    var maxAttempts = DefaultMaxAttempts;

                    for (var attempt = 1; attempt <= maxAttempts; attempt++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            await using var stream = new FileStream(
                                path,
                                FileMode.Open,
                                FileAccess.Read,
                                FileShare.ReadWrite | FileShare.Delete,
                                bufferSize: 81920,
                                useAsync: true
                            );

                            var formattedDate = DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm:ss");
                            var caption =
                                $@"UTC TIME: {formattedDate}
                                FILENAME: {fileName}
                                FULL PATH: {path}";
                            
                            await _bot.SendDocument(_backupSettings.ChannelId, stream, caption);

                            _logger.LogInformation("Backup successful for file {fileName} (attempt {attempt})",
                                fileName, attempt);
                            break; 
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogInformation("Backup canceled during operation for {path}", path);
                            throw; 
                        }
                        catch (Exception ex)
                        {
                            var isLastAttempt = attempt == maxAttempts;
                            _logger.LogError(ex, "Attempt {attempt} failed to backup {path}. {message}", attempt, path,
                                ex.Message);

                            if (isLastAttempt)
                            {
                                _logger.LogError("All {maxAttempts} attempts failed for {path}. Skipping file.",
                                    maxAttempts, path);
                                break;
                            }
                            
                            var delayMs = (int)(DefaultBaseDelayMilliseconds * Math.Pow(2, attempt - 1));
                            _logger.LogInformation(
                                "Waiting {delay}ms before retrying file {path} (attempt {nextAttempt}/{maxAttempts})",
                                delayMs, path, attempt + 1, maxAttempts);

                            try
                            {
                                await Task.Delay(delayMs, cancellationToken);
                            }
                            catch (OperationCanceledException)
                            {
                                _logger.LogInformation("Backup canceled while waiting to retry {path}", path);
                                throw;
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Backup run canceled while processing {path}", path);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while backing up {path}. Continuing with next file.", path);
                }
            }
        }
    }
}