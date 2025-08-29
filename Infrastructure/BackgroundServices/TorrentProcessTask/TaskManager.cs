using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class TaskManager : ITaskManager
    {
        private readonly IDbContextFactory<DownloadContext> _contextFactory;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<TaskManager> _logger;

        public TaskManager(
            IDbContextFactory<DownloadContext> contextFactory,
            IBackgroundJobClient backgroundJobClient,
            ILogger<TaskManager> logger)
        {
            _contextFactory = contextFactory;
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
        }

        public async Task<Result<Unit>> CancelTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var ctx = _contextFactory.CreateDbContext();

            var task = await ctx.TorrentTasks
                .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

            if (task == null)
            {
                return Result<Unit>.Failure("Task not found.");
            }

            if (task.State == TorrentTaskState.Completed ||
                task.State == TorrentTaskState.Cancelled ||
                task.State == TorrentTaskState.Cancelling)
            {
                return Result<Unit>.Failure("Task is already completed, cancelled or in the process of cancelling.");
            }

            // Mark as cancelling
            task.State = TorrentTaskState.Cancelling;
            await ctx.SaveChangesAsync(cancellationToken);

            // Enqueue TaskCleaner into the dedicated "cleaners" queue with CancellationToken.None
            try
            {
                var job = Job.FromExpression<TaskCleaner>(cleaner => cleaner.CleanUpAsync(task.Id, CancellationToken.None));
                _backgroundJobClient.Create(job, new EnqueuedState("cleaners"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to enqueue TaskCleaner for task {TaskId}", task.Id);
            }

            // reflect user requested cancellation
            task.State = TorrentTaskState.Cancelled;
            await ctx.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }

        public async Task<Result<Unit>> RetryTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var ctx = _contextFactory.CreateDbContext();

            var task = await ctx.TorrentTasks
                .Include(t => t.TaskDownloadProgress)
                .Include(t => t.TaskUploadProgress)
                .Include(t => t.SubtitleVideoPairs)
                .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

            if (task == null)
            {
                return Result<Unit>.Failure("Task not found.");
            }

            if (task.State != TorrentTaskState.Error && task.State != TorrentTaskState.Cancelled)
            {
                return Result<Unit>.Failure("Task is not in Error or Canceled state and cannot be retried.");
            }

            if (task.TaskDownloadProgress != null)
            {
                ctx.TaskDownloadProgress.Remove(task.TaskDownloadProgress);
            }

            if (task.TaskUploadProgress != null)
            {
                // TaskUploadProgress appears to be a collection in your previous code; guard just in case
                if (task.TaskUploadProgress is System.Collections.IEnumerable)
                    ctx.TaskUploadProgress.RemoveRange(task.TaskUploadProgress);
                else
                    ctx.TaskUploadProgress.RemoveRange(task.TaskUploadProgress);
            }

            if (task.SubtitleVideoPairs != null && task.SubtitleVideoPairs.Count != 0)
            {
                ctx.SubtitleVideoPairs.RemoveRange(task.SubtitleVideoPairs);
            }

            task.State = TorrentTaskState.Pending;
            task.ErrorMessage = null;
            task.StartTime = null;
            task.EndTime = null;
            task.FileSavingPath = null;
            task.SubtitleSavingPath = null;
            task.UpdatedAt = DateTime.UtcNow;

            var res = await ctx.SaveChangesAsync(cancellationToken) > 0;

            if (!res)
            {
                _logger.LogError("Failed to retry task {TaskId}", task.Id);
                return Result<Unit>.Failure("Failed to retry the task.");
            }

            _logger.LogInformation("Task {TaskId} reset to Pending for retry.", task.Id);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}
