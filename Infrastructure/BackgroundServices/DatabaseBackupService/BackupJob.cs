using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WTelegram;

namespace Infrastructure.BackgroundServices.DatabaseBackupService;

public class BackupJob
{
    private readonly Bot _bot;
    private readonly BackupSettings _backupSettings;
    private readonly ILogger<BackupJob> _logger;
    private readonly IConfiguration _configuration;

    private const int DefaultMaxAttempts = 3;
    private const int DefaultBaseDelayMilliseconds = 1000;

    public BackupJob(Bot bot, IOptions<BackupSettings> backupSettings, IConfiguration configuration,
        ILogger<BackupJob> logger)
    {
        _bot = bot;
        _backupSettings = backupSettings.Value;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartBackup(CancellationToken cancellationToken = default)
    {
        if (!_backupSettings.IsEnabled)
        {
            _logger.LogInformation("Database backup is disabled in settings.");
            return;
        }

        var connSection = _configuration.GetSection("ConnectionStrings");
        if (!connSection.Exists())
        {
            _logger.LogWarning("No ConnectionStrings section found in configuration. Skipping backups.");
            return;
        }

        var connections = connSection.GetChildren();
        foreach (var entry in connections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var connName = entry.Key;
            var connStr = entry.Value;

            if (string.IsNullOrWhiteSpace(connStr))
            {
                _logger.LogWarning("Connection string '{connName}' is empty. Skipping.", connName);
                continue;
            }

            // Quick heuristic: only try backing up SQLite connection strings
            if (!connStr.Contains("Data Source", StringComparison.OrdinalIgnoreCase) &&
                !connStr.Contains(".db", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Skipping non-SQLite connection '{connName}'", connName);
                continue;
            }

            SqliteConnectionStringBuilder sourceBuilder;
            try
            {
                sourceBuilder = new SqliteConnectionStringBuilder(connStr);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Connection string for '{connName}' is not a valid SQLite connection string. Skipping.", connName);
                continue;
            }

            // Ensure readonly mode for the source
            sourceBuilder["Mode"] = "ReadOnly";
            sourceBuilder["Cache"] = "Shared";

            var dataSource = sourceBuilder.DataSource ?? connName;
            var fileName = Path.GetFileName(dataSource);

            _logger.LogInformation("Beginning backup for connection '{connName}' -> DataSource: {dataSource}", connName,
                dataSource);

            for (var attempt = 1; attempt <= DefaultMaxAttempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // temp file will hold the consistent snapshot produced by SQLite's backup API
                var tempFilePath = Path.Combine(Path.GetTempPath(), $"backup_{connName}_{Guid.NewGuid():N}.db");

                try
                {
                    var destBuilder = new SqliteConnectionStringBuilder
                    {
                        DataSource = tempFilePath
                    };
                    destBuilder["Mode"] = "ReadWriteCreate";
                    destBuilder["Cache"] = "Shared";

                    await using var source = new SqliteConnection(sourceBuilder.ToString());
                    await using var destination = new SqliteConnection(destBuilder.ToString());

                    await source.OpenAsync(cancellationToken);
                    await destination.OpenAsync(cancellationToken);

                    // BackupDatabase is synchronous in the provider - run on a threadpool thread
                    await Task.Run(() => source.BackupDatabase(destination), cancellationToken);

                    // Open the temp file and send it to Telegram
                    await using var stream = new FileStream(
                        tempFilePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read,
                        81920,
                        true
                    );

                    var formattedDate = DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss");
                    var caption =
                        $"""
                         UTC_TIME: {formattedDate}
                         CONNECTION_NAME: {connName}
                         FILENAME: {fileName}
                         DATA_SOURCE: {dataSource}
                         BACKUP_SIZE_BYTES: {stream.Length}
                         """;

                    await _bot.SendDocument(_backupSettings.ChannelId, stream, caption);

                    _logger.LogInformation("Backup successful for connection {connName} (attempt {attempt})", connName,
                        attempt);

                    // cleanup temp file
                    try
                    {
                        stream.Close();
                        if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not delete temporary backup file {tempFilePath}", tempFilePath);
                    }

                    break; // success -> exit retry loop
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Backup canceled during operation for connection {connName}", connName);
                    throw;
                }
                catch (Exception ex)
                {
                    var isLastAttempt = attempt == DefaultMaxAttempts;
                    _logger.LogError(ex, "Attempt {attempt} failed to backup connection {connName}. {message}", attempt,
                        connName, ex.Message);

                    // try to remove temp file if it exists
                    try
                    {
                        if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
                    }
                    catch
                    {
                        /* best-effort cleanup */
                    }

                    if (isLastAttempt)
                    {
                        _logger.LogError("All {maxAttempts} attempts failed for connection {connName}. Skipping.",
                            DefaultMaxAttempts, connName);
                        break;
                    }

                    var delayMs = (int)(DefaultBaseDelayMilliseconds * Math.Pow(2, attempt - 1));
                    _logger.LogInformation(
                        "Waiting {delay}ms before retrying connection {connName} (attempt {nextAttempt}/{maxAttempts})",
                        delayMs, connName, attempt + 1, DefaultMaxAttempts);

                    try
                    {
                        await Task.Delay(delayMs, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Backup canceled while waiting to retry connection {connName}",
                            connName);
                        throw;
                    }
                }
            }
        }
    }
}