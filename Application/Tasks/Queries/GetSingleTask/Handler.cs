using Application.Core;
using Application.Tasks.Queries.GetTasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Tasks.Queries.GetSingleTask
{
    public class Handler : IRequestHandler<Query, Result<TaskDto>>
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

        public async Task<Result<TaskDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var task = await _downloadContext.TorrentTasks
                .Include(t => t.TaskDownloadProgress)
                .Include(t => t.TaskUploadProgress)
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (task == null)
            {
                return Result<TaskDto>.Failure("Task not found.");
            }

            var user = await _dataContext.Users
                .FirstOrDefaultAsync(u => u.Id == task.UserId, cancellationToken);

            var taskDto = _mapper.Map<TaskDto>(task);

            if (user != null)
            {
                taskDto.UserName = user.UserName;
            }

            return Result<TaskDto>.Success(taskDto);
        }
    }
}
