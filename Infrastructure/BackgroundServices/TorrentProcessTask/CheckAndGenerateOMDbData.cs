using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.OmdbWrapper;
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

        public async Task<bool> Checker(Guid taskId)
        {
            if (taskId == Guid.Empty)
            {
                return false;
            }

            var task = await _downloadContext.TorrentTasks
                .FirstOrDefaultAsync(x => x.Id == taskId);

            if (task == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(task.ImdbId))
            {
                task.ErrorMessage = "Missing IMDb ID.";
                task.State = Domain.Enums.TorrentTaskState.Error;
                await _downloadContext.SaveChangesAsync();
                return false;
            }

            var existingItem = await _dataContext.OmdbItems
                .FirstOrDefaultAsync(x => x.ImdbId == task.ImdbId);

            if (existingItem != null)
            {
                task.State = TorrentTaskState.InGettingOmdbDetailsProcess;
                await _downloadContext.SaveChangesAsync();
                return true;
            }

            try
            {
                var newItem = await _OMDbService.GetTitleByImdbIdAsync(task.ImdbId);

                if (newItem == null)
                {
                    task.ErrorMessage = $"OMDb returned null for IMDb ID: {task.ImdbId}";
                    task.State = TorrentTaskState.Error;
                    await _downloadContext.SaveChangesAsync();
                    return false;
                }

                _dataContext.OmdbItems.Add(newItem);
                await _dataContext.SaveChangesAsync();

                task.State = TorrentTaskState.InGettingOmdbDetailsProcess;
                await _downloadContext.SaveChangesAsync();

                return true;
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

            await _downloadContext.SaveChangesAsync();
            return false;
        }
    }
}
