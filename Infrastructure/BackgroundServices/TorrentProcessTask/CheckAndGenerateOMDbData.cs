using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class CheckAndGenerateOMDbData
    {
        private readonly DownloadContext _downloadContext;
        private readonly IOmdbService _OMDbService;
        private readonly DataContext _dataContext;

        public CheckAndGenerateOMDbData(
            DownloadContext downloadContext,
            DataContext dataContext,
            IOmdbService OMDbService)
        {
            _dataContext = dataContext;
            _OMDbService = OMDbService;
            _downloadContext = downloadContext;
        }

        public async Task<bool> Checker(Guid taskId, CancellationToken cancellationToken = default)
        {
            // Early checks & cancellation
            cancellationToken.ThrowIfCancellationRequested();

            if (taskId == Guid.Empty)
            {
                return false;
            }

            var task = await _downloadContext.TorrentTasks
                .FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);

            if (task == null)
            {
                return false;
            }

            // Another quick cancellation point
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(task.ImdbId))
            {
                task.ErrorMessage = "Missing IMDb ID.";
                task.State = Domain.Enums.TorrentTaskState.Error;
                await _downloadContext.SaveChangesAsync(cancellationToken);
                return false;
            }

            var existingItem = await _dataContext.OmdbItems
                .FirstOrDefaultAsync(x => x.ImdbId == task.ImdbId, cancellationToken);

            if (existingItem != null)
            {
                task.State = TorrentTaskState.InGettingOmdbDetailsProcess;
                await _downloadContext.SaveChangesAsync(cancellationToken);
                return true;
            }

            try
            {
                // Allow cancellation before calling external service
                cancellationToken.ThrowIfCancellationRequested();

                // If your IOmdbService supports a CancellationToken overload, prefer calling it with the token.
                // Here we call the existing API as-is to avoid breaking changes.
                var newItem = await _OMDbService.GetTitleByImdbIdAsync(task.ImdbId);

                // Check cancellation after external call (in case it was cancelled while waiting)
                cancellationToken.ThrowIfCancellationRequested();

                if (newItem == null)
                {
                    task.ErrorMessage = $"OMDb returned null for IMDb ID: {task.ImdbId}";
                    task.State = TorrentTaskState.Error;
                    await _downloadContext.SaveChangesAsync(cancellationToken);
                    return false;
                }

                _dataContext.OmdbItems.Add(newItem);
                await _dataContext.SaveChangesAsync(cancellationToken);

                task.State = TorrentTaskState.InGettingOmdbDetailsProcess;
                await _downloadContext.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (OperationCanceledException)
            {
                // Propagate cancellation so the outer job can handle it.
                throw;
            }
            catch (InvalidOperationException ex)
            {
                task.ErrorMessage = $"OMDb API key issue: {ex.Message}";
                task.State = TorrentTaskState.Error;
            }
            catch (Exception ex)
            {
                task.ErrorMessage = $"Unhandled OMDb error: {ex.Message}";
                task.State = TorrentTaskState.Error;
            }

            await _downloadContext.SaveChangesAsync(cancellationToken);
            return false;
        }
    }
}
