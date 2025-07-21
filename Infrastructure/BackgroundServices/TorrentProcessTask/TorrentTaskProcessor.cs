using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{

    public class TorrentTaskProcessor
    {
        private readonly DownloadContext _downloadContext;
        private readonly ILogger<TorrentTaskProcessor> _logger;

        public TorrentTaskProcessor(DownloadContext context, ILogger<TorrentTaskProcessor> logger)
        {
            _downloadContext = context;
            _logger = logger;
        }

        public async Task Process(int taskId)
        {
            var task = await _downloadContext.TorrentTasks.FindAsync(taskId);

            if (task == null)
            {
                _logger.LogWarning("Task {TaskId} not valid or not ready.", taskId);
                return;
            }

            _logger.LogInformation("Processing Task {TaskId}", task.Id);


            await _downloadContext.SaveChangesAsync();
        }
    }

}