using Domain.Enums;
using Infrastructure.BackgroundServices.Models;
using Infrastructure.Utilities;
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

        public async Task<bool> CheckAsync(Guid torrentTaskId)
        {   
            var task = await _context.TorrentTasks.FindAsync(torrentTaskId);

            if (task == null)
            {
                _logger.LogWarning("There is no Task with this Id: {Id}", torrentTaskId);
                return false;
            }

            if (SystemResources.GetCurrentDriveFreeSpace() < _settings.MinimumRequiredSpace)
            {
                throw new Exception(
                    "Not enough free disk space to process torrent. The job will be retried later.");
            }

            task.StartTime = DateTime.UtcNow;
            task.State = TorrentTaskState.JobStarted;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
