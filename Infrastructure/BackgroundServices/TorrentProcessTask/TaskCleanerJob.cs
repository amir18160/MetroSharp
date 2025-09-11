using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.BackgroundServices.TorrentProcessTask;

public class TaskCleanerJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TaskCleanerJob> _logger;

    public TaskCleanerJob(IServiceProvider serviceProvider, ILogger<TaskCleanerJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var idString = context.JobDetail.JobDataMap.GetString("taskId");
        if (!Guid.TryParse(idString, out var taskId))
        {
            _logger.LogError("TaskCleanerJob invoked without valid taskId.");
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var cleaner = scope.ServiceProvider.GetRequiredService<TaskCleaner>();
            await cleaner.CleanUpAsync(taskId, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running TaskCleaner for task {TaskId}", idString);
        }
    }
}