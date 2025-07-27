using Application.Core;
using Domain.Enums;
using MediatR;

namespace Application.Tasks.Queries.GetTasks
{
    public class Query : PagingParams, IRequest<Result<PagedList<TaskDto>>>
    {
        public string Title { get; set; }
        public string ImdbId { get; set; }
        public string TorrentHash { get; set; }
        public TorrentTaskState? State { get; set; }
        public TorrentTaskType? TaskType { get; set; }
        public TorrentTaskPriority? Priority { get; set; }
        public bool? HasError { get; set; }
        public string UserId { get; set; }
        public DateFilter CreatedAt { get; set; }
        public DateFilter UpdatedAt { get; set; }
    }
}
