using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence;
using Domain.Entities;
using Application.Core;
using Application.Interfaces;
using Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.ActiveDeactive
{

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, UserManager<User> userManager, IUserAccessor userAccessor)
        {
            _context = context;
            _userManager = userManager;
            _userAccessor = userAccessor;
        }
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var targetUser = await _userManager.FindByNameAsync(request.Username);
            if (targetUser == null)
                return Result<Unit>.Failure("Target user not found.");

            var targetRoles = await _userManager.GetRolesAsync(targetUser);
            if (targetRoles.Any(role => role is "Admin" or "Owner"))
                return Result<Unit>.Failure("An admin account cannot be disabled.");

            var currentUsername = _userAccessor.GetUserName();
            if (string.IsNullOrWhiteSpace(currentUsername))
                return Result<Unit>.Failure("User not authenticated.");

            if (targetUser.UserName == currentUsername)
                return Result<Unit>.Failure("You cannot activate or deactivate your own account.");

            var currentUser = await _userManager.FindByNameAsync(currentUsername);
            var isAdminOrOwner = currentUser != null &&
                                 (await _userManager.IsInRoleAsync(currentUser, Roles.Admin) ||
                                  await _userManager.IsInRoleAsync(currentUser, Roles.Owner));

            if (!isAdminOrOwner)
                return Result<Unit>.Failure("Only Owner or Admin can activate or deactivate users.");

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == targetUser.Id, cancellationToken: cancellationToken);

            if (user == null)
                return Result<Unit>.Failure("User record not found in context.");

            if (user.IsActive == request.Activate)
                return Result<Unit>.Failure($"User is already {(request.Activate ? "active" : "inactive")}.");

            user.IsActive = request.Activate;

            var success = await _context.SaveChangesAsync(cancellationToken) > 0;
            return success
                ? Result<Unit>.Success(Unit.Value)
                : Result<Unit>.Failure($"Failed to {(request.Activate ? "activate" : "deactivate")} user.");
        }

    }
}
