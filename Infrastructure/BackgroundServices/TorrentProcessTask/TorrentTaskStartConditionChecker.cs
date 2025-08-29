using Domain.Enums;
using Hangfire;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class TorrentTaskStartConditionChecker
    {
        private readonly DownloadContext _context;
        private readonly ILogger<TorrentTaskStartConditionChecker> _logger;
        private readonly TorrentTaskSettings _settings;

        public TorrentTaskStartConditionChecker(
            DownloadContext context,
            ILogger<TorrentTaskStartConditionChecker> logger,
            IOptions<TorrentTaskSettings> settings)
        {
            _context = context;
            _logger = logger;
            _settings = settings.Value;
        }
        
        public async Task<bool> CheckAsync(Guid torrentTaskId, CancellationToken cancellationToken = default)
        {
            // Respect cancellation early
            cancellationToken.ThrowIfCancellationRequested();

            // Use FindAsync with cancellation token
            var task = await _context.TorrentTasks.FirstOrDefaultAsync(x=> x.Id == torrentTaskId , cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("There is no Task with this Id: {Id}", torrentTaskId);
                return false;
            }

            if (task.State != TorrentTaskState.JobQueue)
            {
                _logger.LogWarning("Task is not in the correct state to start. Task Id: {Id}", torrentTaskId);
                _logger.LogWarning("This message might be because multiple worker running the same job"); 
                return false;
            }

            // Check cancellation before doing potentially blocking / long-running checks
            cancellationToken.ThrowIfCancellationRequested();

            if (SystemResources.GetCurrentDriveFreeSpace() < _settings.MinimumRequiredSpace)
            {
                throw new Exception(
                    "Not enough free disk space to process torrent. The job will be retried later.");
            }

            task.StartTime = DateTime.UtcNow;
            task.State = TorrentTaskState.JobStarted;

            // Persist changes with cancellation token
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
