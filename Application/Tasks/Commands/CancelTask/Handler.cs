using Application.Core;
using Application.Interfaces;
using Domain.Core;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Tasks.Commands.CancelTask
{
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly ITaskManager _taskManager;
        private readonly IUserAccessor _userAccessor;
        private readonly UserManager<User> _userManager;
        private readonly DownloadContext _downloadContext;
        private readonly DataContext _dataContext;

        public Handler(ITaskManager taskManager, IUserAccessor userAccessor, UserManager<User> userManager, DownloadContext downloadContext, DataContext dataContext)
        {
            _taskManager = taskManager;
            _userAccessor = userAccessor;
            _userManager = userManager;
            _downloadContext = downloadContext;
            _dataContext = dataContext;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var taskToCancel = await _downloadContext.TorrentTasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
            if (taskToCancel == null)
            {
                return Result<Unit>.Failure("Task not found.");
            }

            var user = await _userManager.FindByNameAsync(_userAccessor.GetUserName());
            if (user == null)
            {
                return Result<Unit>.Failure("User not found.");
            }

            var isOwner = await _userManager.IsInRoleAsync(user, Roles.Owner);

            if (taskToCancel.UserId != user.Id && !isOwner)
            {
                return Result<Unit>.Failure("You are not authorized to cancel this task.");
            }

            return await _taskManager.CancelTaskAsync(request.Id, cancellationToken);
        }
    }
}