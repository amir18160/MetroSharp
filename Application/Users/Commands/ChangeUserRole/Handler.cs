using Application.Core;
using Application.Interfaces;
using Domain.Core;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence;

namespace Application.Users.Commands.ChangeUserRole;

public class Handler : IRequestHandler<Command, Result<Unit>>
{
    private readonly DataContext _context;
    private readonly IUserAccessor _userAccessor;
    private readonly UserManager<User> _userManager;

    public Handler(DataContext context, UserManager<User> userManager, IUserAccessor userAccessor)
    {
        _context = context;
        _userManager = userManager;
        _userAccessor = userAccessor;
    }

    public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
    {
        var targetUser = await _userManager.FindByIdAsync(request.UserId);

        if (targetUser == null) return Result<Unit>.Failure("Target user not found.");

        var currentUsername = _userAccessor.GetUserName();

        if (targetUser.UserName == currentUsername) return Result<Unit>.Failure("You cannot change your own role.");

        var currentRoles = await _userManager.GetRolesAsync(targetUser);

        var removeResult = await _userManager.RemoveFromRolesAsync(targetUser, currentRoles);
        if (!removeResult.Succeeded) return Result<Unit>.Failure("Failed to remove existing roles.");


        var addResult = await _userManager.AddToRoleAsync(targetUser, request.NewRole);
        
        if (!addResult.Succeeded) return Result<Unit>.Failure("Failed to add new role.");

        if (request.NewRole == Roles.Admin && addResult.Succeeded && !targetUser.IsConfirmed)
        {
            targetUser.IsConfirmed = true;
            await _userManager.UpdateAsync(targetUser);
        }

        return Result<Unit>.Success(Unit.Value);
    }
}