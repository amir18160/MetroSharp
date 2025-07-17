using Application.Core;
using Application.Interfaces;
using Domain.Core;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.BannedUsers.Commands.BanUser
{
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly IUserAccessor _userAccessor;
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        public Handler(IUserAccessor userAccessor, DataContext context, UserManager<User> userManager)
        {
            _userManager = userManager;
            _context = context;
            _userAccessor = userAccessor;
        }


        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == request.UserId, cancellationToken: cancellationToken);
            if (user == null)
            {
                return Result<Unit>.Failure("No user found with that id.");
            }

            if (user.UserName == _userAccessor.GetUserName())
            {
                return Result<Unit>.Failure("You cannot ban yourself.");
            }

            var targetRoles = await _userManager.GetRolesAsync(user);
            if (targetRoles.Any(role => role is Roles.Admin or Roles.Owner))
            {
                return Result<Unit>.Failure("You cannot ban admins or owners.");
            }

            var alreadyBanned = await _context.BannedUsers
                .FirstOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);

            if (alreadyBanned != null)
            {
                if (alreadyBanned.FreeAt == null)
                {
                    return Result<Unit>.Failure("User is already banned indefinitely.");
                }

                if (request.FreeAt != null && request.FreeAt > alreadyBanned.FreeAt)
                {
                    alreadyBanned.FreeAt = request.FreeAt;
                }

                alreadyBanned.Reason = $"{alreadyBanned.Reason} | {request.Reason}";
                alreadyBanned.BannedAt = DateTime.UtcNow;

                var success = await _context.SaveChangesAsync(cancellationToken) > 0;
                return success
                    ? Result<Unit>.Success(Unit.Value)
                    : Result<Unit>.Failure("Failed to update existing ban.");
            }

            var newBan = new BannedUser
            {
                UserId = user.Id,
                Reason = request.Reason,
                BannedAt = DateTime.UtcNow,
                FreeAt = request.FreeAt
            };

            _context.BannedUsers.Add(newBan);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;
            return result
                ? Result<Unit>.Success(Unit.Value)
                : Result<Unit>.Failure("Failed to ban user.");
        }

    }
}