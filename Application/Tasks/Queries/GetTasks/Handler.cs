using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Tasks.Queries.GetTasks
{
    public class Handler : IRequestHandler<Query, Result<PagedList<TaskDto>>>
    {
        private readonly DownloadContext _downloadContext;
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public Handler(DownloadContext downloadContext, DataContext dataContext, IMapper mapper)
        {
            _downloadContext = downloadContext;
            _dataContext = dataContext;
            _mapper = mapper;
        }

        public async Task<Result<PagedList<TaskDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _downloadContext.TorrentTasks
                .Include(t => t.TaskDownloadProgress)
                .Include(t => t.TaskUploadProgress)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Title))
                query = query.Where(t => t.Title.Contains(request.Title));

            if (!string.IsNullOrWhiteSpace(request.ImdbId))
                query = query.Where(t => t.ImdbId == request.ImdbId);
            
            if (!string.IsNullOrWhiteSpace(request.TorrentHash))
                query = query.Where(t => t.TorrentHash == request.TorrentHash);

            if (request.State.HasValue)
                query = query.Where(t => t.State == request.State.Value);

            if (request.TaskType.HasValue)
                query = query.Where(t => t.TaskType == request.TaskType.Value);

            if (request.Priority.HasValue)
                query = query.Where(t => t.Priority == request.Priority.Value);
            
            if (request.HasError.HasValue)
                query = query.Where(t => request.HasError.Value ? t.ErrorMessage != null : t.ErrorMessage == null);

            if (!string.IsNullOrWhiteSpace(request.UserId))
                query = query.Where(t => t.UserId == request.UserId);

            if (request.CreatedAt != null)
                query = query.ApplyDateFilter(request.CreatedAt, t => t.CreatedAt);

            if (request.UpdatedAt != null)
                query = query.ApplyDateFilter(request.UpdatedAt, t => t.UpdatedAt);

            var tasksWithUsers = await query
                .Join(_dataContext.Users,
                      task => task.UserId,
                      user => user.Id,
                      (task, user) => new { Task = task, User = user })
                .ToListAsync(cancellationToken);

            var taskDtos = _mapper.Map<List<TaskDto>>(tasksWithUsers.Select(x => x.Task));
            
            for(int i = 0; i < taskDtos.Count; i++)
            {
                taskDtos[i].UserName = tasksWithUsers[i].User.UserName;
            }

            var pagedList = await PagedList<TaskDto>.CreateAsync(taskDtos.AsQueryable(), request.PageNumber, request.PageSize);
            
            return Result<PagedList<TaskDto>>.Success(pagedList);
        }
    }
}
