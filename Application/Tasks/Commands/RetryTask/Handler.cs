using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public Handler(DownloadContext downloadContext, ILogger<Handler> logger, IUserAccessor userAccessor, UserManager<User> userManager)
        {
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
                .Include(x => x.TaskDownloadProgress)
                .Include(x => x.SubtitleVideoPairs)
                .Include(x => x.TaskUploadProgress)
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

            if (task.State != TorrentTaskState.Error && task.State != TorrentTaskState.Cancelled)
            {
                return Result<Unit>.Failure("Task is not in Error or Canceled state and cannot be retried.");
            }

            if (task.TaskDownloadProgress != null)
            {
                _downloadContext.TaskDownloadProgress.Remove(task.TaskDownloadProgress);
            }

            if (task.TaskUploadProgress != null)
            {
                _downloadContext.TaskUploadProgress.RemoveRange(task.TaskUploadProgress);
            }

            if (task.SubtitleVideoPairs != null && task.SubtitleVideoPairs.Count != 0)
            {
                _downloadContext.SubtitleVideoPairs.RemoveRange(task.SubtitleVideoPairs);
            }

            task.State = TorrentTaskState.Pending;
            task.ErrorMessage = null;
            task.StartTime = null;
            task.EndTime = null;
            task.FileSavingPath = null;
            task.SubtitleSavingPath = null;
            task.UpdatedAt = DateTime.UtcNow;

            var res = await _downloadContext.SaveChangesAsync(cancellationToken) > 0;

            if (!res)
            {
                _logger.LogError("Failed to retry task {TaskId}", task.Id);
                return Result<Unit>.Failure("Failed to retry the task.");
            }

            return Result<Unit>.Success(Unit.Value);
        }
    }
}