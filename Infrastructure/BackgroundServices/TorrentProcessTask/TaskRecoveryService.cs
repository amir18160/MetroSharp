using Domain.Enums;
using Infrastructure.BackgroundServices.TorrentProcessTask;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.BackgroundServices
{
    public static class TaskRecovery
    {
        public static async Task RecoverInterruptedTasks(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<TorrentTaskProcessor>>();
            var downloadContext = scope.ServiceProvider.GetRequiredService<DownloadContext>();
            var taskCleaner = scope.ServiceProvider.GetRequiredService<TaskCleaner>(); // Resolve the cleaner

            logger.LogInformation("--- Starting Application Startup Task Recovery ---");

            var incompleteTasks = await downloadContext.TorrentTasks
                .Where(t => t.State != TorrentTaskState.Completed &&
                            t.State != TorrentTaskState.Pending &&
                            t.State != TorrentTaskState.Error)
                .ToListAsync();

            if (!incompleteTasks.Any())
            {
                logger.LogInformation("No interrupted tasks found to recover.");
                logger.LogInformation("--- Task Recovery Finished ---");
                return;
            }

            logger.LogWarning("Found {Count} tasks that did not finish correctly. Cleaning and recovering...", incompleteTasks.Count);

            foreach (var task in incompleteTasks)
            {
                logger.LogWarning("Recovering task {TaskId} which was stuck in state: {State}", task.Id, task.State);

                // *** THIS IS THE KEY CHANGE ***
                // Directly execute the cleanup logic instead of enqueuing it.
                await taskCleaner.CleanUpAsync(task.Id);

                // Now, safely reset the task's state to Pending.
                task.State = TorrentTaskState.Pending;
                task.ErrorMessage = $"Task automatically recovered after application restart from state: {task.State}";
                task.UpdatedAt = DateTime.UtcNow;
            }

            await downloadContext.SaveChangesAsync();
            logger.LogInformation("--- Task Recovery Finished ---");
        }
    }
}