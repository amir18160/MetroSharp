using Persistence;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class TorrentProcessTask
    {
        private readonly DownloadContext _downloadContext;
        public TorrentProcessTask(DownloadContext downloadContext)
        {
            _downloadContext = downloadContext;
            
        }
        public async Task ProcessTorrentTaskAsync(Guid torrentTaskId)
        {
            var task = await _downloadContext.TorrentTasks.FindAsync(torrentTaskId);

            if (task == null)
            {
                // Optionally: retry later or discard
                return;
            }

            // Your task logic
            Console.WriteLine($"Running task {torrentTaskId}");

            // Mark task as completed or update status in DB
        }
    }
}
