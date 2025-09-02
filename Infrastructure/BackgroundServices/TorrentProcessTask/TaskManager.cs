using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Quartz;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class TaskManager : ITaskManager
    {
        private readonly IDbContextFactory<DownloadContext> _contextFactory;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ILogger<TaskManager> _logger;

        public TaskManager(
            IDbContextFactory<DownloadContext> contextFactory,
            ISchedulerFactory schedulerFactory,
            ILogger<TaskManager> logger)
        {
            _contextFactory = contextFactory;
            _schedulerFactory = schedulerFactory;
            _logger = logger;
        }

        public async Task<Result<Unit>> CancelTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);

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

            // Mark as cancelling in DB first
            task.State = TorrentTaskState.Cancelling;
            await ctx.SaveChangesAsync(cancellationToken);

            // Obtain scheduler lazily
            IScheduler scheduler = null;
            try
            {
                scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to obtain scheduler instance while cancelling task {TaskId}", task.Id);
            }

            // Schedule the TaskCleaner job immediately as a best-effort backup.
            // We schedule it BEFORE the interrupt in case the interrupt doesn't take effect promptly.
            try
            {
                if (scheduler != null)
                {
                    var cleanerJobKey = new JobKey($"cleaner-{task.Id}", "cleaners");

                    // Build the cleaner job detail
                    var cleanerJob = JobBuilder.Create<TaskCleanerJob>()
                        .WithIdentity(cleanerJobKey)
                        .UsingJobData("taskId", task.Id.ToString())
                        .StoreDurably(false)
                        .Build();

                    var cleanerTrigger = TriggerBuilder.Create()
                        .WithIdentity($"trigger-cleaner-{task.Id}", "cleaners")
                        .StartNow()
                        .Build();

                    // Schedule the cleaner
                    await scheduler.ScheduleJob(cleanerJob, cleanerTrigger, cancellationToken);
                    _logger.LogInformation("Scheduled cleaner job for task {TaskId}", task.Id);
                }
                else
                {
                    _logger.LogWarning("Scheduler is null — cannot schedule cleaner job for task {TaskId}", task.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to schedule TaskCleaner for task {TaskId}", task.Id);
            }

            // Attempt to interrupt the running job (cooperative cancellation)
            try
            {
                if (scheduler != null)
                {
                    var jobKey = new JobKey($"torrent-{task.Id}", "torrent-jobs");

                    if (await scheduler.CheckExists(jobKey, cancellationToken))
                    {
                        // Log currently executing jobs for diagnostics
                        try
                        {
                            var executing = await scheduler.GetCurrentlyExecutingJobs(cancellationToken);
                            var isExecuting = executing.Any(j => j.JobDetail.Key.Equals(jobKey));
                            _logger.LogInformation(
                                "Cancelling task {TaskId}. Job exists: true, currently executing: {IsExecuting}",
                                task.Id, isExecuting);
                        }
                        catch (Exception exExec)
                        {
                            _logger.LogDebug(exExec, "Unable to read currently executing jobs for diagnostics.");
                        }

                        // Request cooperative cancellation — this cancels IJobExecutionContext.CancellationToken in the job
                        await scheduler.Interrupt(jobKey, cancellationToken);
                        _logger.LogInformation("Interrupt requested for job {JobKey}", jobKey);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Job {JobKey} does not exist (it may not have been scheduled yet or already finished).",
                            $"torrent-{task.Id}");
                    }

                    // Optionally delete the job to prevent further runs or rescheduling
                    try
                    {
                        var jobKey2 = new JobKey($"torrent-{task.Id}", "torrent-jobs");
                        if (await scheduler.CheckExists(jobKey2, cancellationToken))
                        {
                            await scheduler.DeleteJob(jobKey2, cancellationToken);
                            _logger.LogInformation("Deleted job {jobKey2} after interrupt attempt.", jobKey2);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Failed to delete job after interrupt attempt. (non-fatal)");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to interrupt running job for task {TaskId}", task.Id);
            }

            // reflect user requested cancellation in DB (persist final state)
            try
            {
                task.State = TorrentTaskState.Cancelled;
                await ctx.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to persist Cancelled state for task {TaskId}", task.Id);
            }

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