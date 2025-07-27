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

            // Get the total count for pagination before applying skip and take
            var count = await query.CountAsync(cancellationToken);

            // Now, apply pagination to the query
            var pagedTasks = await query.Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);
            
            // Get the user IDs from the paginated list of tasks
            var userIds = pagedTasks.Select(t => t.UserId).Distinct().ToList();

            // Fetch the corresponding users
            var users = await _dataContext.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, cancellationToken);

            var taskDtos = _mapper.Map<List<TaskDto>>(pagedTasks);
            
            // Add the UserName to each DTO
            foreach(var taskDto in taskDtos)
            {
                if(users.TryGetValue(taskDto.UserId, out var user))
                {
                    taskDto.UserName = user.UserName;
                }
            }
            
            var pagedList = new PagedList<TaskDto>(taskDtos, count, request.PageNumber, request.PageSize);
            
            return Result<PagedList<TaskDto>>.Success(pagedList);
        }
    }
}