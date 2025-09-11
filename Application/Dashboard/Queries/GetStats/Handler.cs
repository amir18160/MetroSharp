using Application.Core;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Dashboard.Queries.GetStats;

public class Handler : IRequestHandler<Query, Result<StatsDto>>
{
    private readonly DataContext _dataContext;
    private readonly DownloadContext _downloadContext;


    public Handler(DownloadContext downloadContext, DataContext dataContext)
    {
        _downloadContext = downloadContext;
        _dataContext = dataContext;
    }

    public async Task<Result<StatsDto>> Handle(Query request, CancellationToken cancellationToken)
    {
        var result = new StatsDto();

        result.TotalDocumentsInDatabase = await _dataContext.Documents.CountAsync(cancellationToken);
        result.TotalDocumentsAddedLast24Hours = await _dataContext.Documents
            .Where(x => x.CreatedAt > DateTime.UtcNow.AddHours(-24))
            .CountAsync(cancellationToken);
        result.TotalDocumentsAddedLastWeek = await _dataContext.Documents
            .Where(x => x.CreatedAt > DateTime.UtcNow.AddDays(-7))
            .CountAsync(cancellationToken);
        result.TotalDocumentsAddedLastMonth = await _dataContext.Documents
            .Where(x => x.CreatedAt > DateTime.UtcNow.AddMonths(-1))
            .CountAsync(cancellationToken);
        result.TotalUniqueMoviesInDatabase = await _dataContext.OmdbItems
            .Where(x => x.Documents.Any()).CountAsync(cancellationToken);
        result.TotalUniqueSeriesInDatabase = await _dataContext.OmdbItems
            .Where(x => x.Seasons.Any()).CountAsync(cancellationToken);

        var finishedStates = new List<TorrentTaskState>
            { TorrentTaskState.Cancelled, TorrentTaskState.Completed, TorrentTaskState.Error };

        result.CurrentTaskInProcess = await _downloadContext.TorrentTasks
            .Where(x => !finishedStates.Contains(x.State))
            .CountAsync(cancellationToken);
        result.CurrentTaskInQueue = await _downloadContext.TorrentTasks
            .Where(x => x.State == TorrentTaskState.Pending)
            .CountAsync(cancellationToken);
        result.TotalFailedTasks = await _downloadContext.TorrentTasks
            .Where(x => x.State == TorrentTaskState.Error || x.State == TorrentTaskState.Cancelled)
            .CountAsync(cancellationToken);
        result.TotalFailedTasksLast24Hours = await _downloadContext.TorrentTasks
            .Where(x => (x.State == TorrentTaskState.Error || x.State == TorrentTaskState.Cancelled) &&
                        x.CreatedAt > DateTime.UtcNow.AddHours(-24))
            .CountAsync(cancellationToken);
        result.TotalSuccessfulTasks = await _downloadContext.TorrentTasks
            .Where(x => x.State == TorrentTaskState.Completed)
            .CountAsync(cancellationToken);
        result.TotalSuccessfulTasksLast24Hours = await _downloadContext.TorrentTasks
            .Where(x => x.State == TorrentTaskState.Completed && x.CreatedAt > DateTime.UtcNow.AddHours(-24))
            .CountAsync(cancellationToken);

        return Result<StatsDto>.Success(result);
    }
}