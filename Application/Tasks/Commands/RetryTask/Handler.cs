using Application.Core;
using Application.Interfaces;
using Domain.Core;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Tasks.Commands.RetryTask
{
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DownloadContext _downloadContext;
        private readonly ILogger<Handler> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IUserAccessor _userAccessor;
        private readonly ITaskManager _taskManager;
        public Handler(DownloadContext downloadContext, ILogger<Handler> logger, IUserAccessor userAccessor, UserManager<User> userManager, ITaskManager taskManager)
        {
            _taskManager = taskManager;
            _userAccessor = userAccessor;
            _userManager = userManager;
            _logger = logger;
            _downloadContext = downloadContext;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUserName(), cancellationToken);

            if (user == null)
            {
                return Result<Unit>.Failure("No user found with that id.");
            }

            var targetRoles = await _userManager.GetRolesAsync(user);

            var task = await _downloadContext.TorrentTasks
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (task == null)
            {
                return Result<Unit>.Failure("Task does not exist.");
            }

            var isUserAllowedToRetry = task.UserId == user.Id || targetRoles.Contains(Roles.Owner);
            if (!isUserAllowedToRetry)
            {
                return Result<Unit>.Failure("You are not allowed to retry this task.");
            }

            var result = await _taskManager.RetryTaskAsync(request.Id, cancellationToken);

            return result;
        }
    }
}