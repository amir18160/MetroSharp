using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence;
using Domain.Entities;
using Application.Core;
using Application.Interfaces;
using Domain.Core;

namespace Application.Users.Commands.ChangeUserRole
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
            {
                return Result<Unit>.Failure("Target user not found.");
            }

            var currentUsername = _userAccessor.GetUserName();

            if (targetUser.UserName == currentUsername)
            {
                return Result<Unit>.Failure("You cannot change your own role.");
            }

            var currentRoles = await _userManager.GetRolesAsync(targetUser);

            var removeResult = await _userManager.RemoveFromRolesAsync(targetUser, currentRoles);
            if (!removeResult.Succeeded)
            {
                return Result<Unit>.Failure("Failed to remove existing roles.");
            }


            var addResult = await _userManager.AddToRoleAsync(targetUser, request.NewRole.ToString());
            if (!addResult.Succeeded)
            {
                return Result<Unit>.Failure("Failed to add new role.");
            }

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
