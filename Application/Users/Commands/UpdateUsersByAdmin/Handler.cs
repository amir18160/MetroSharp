using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain.Core;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Users.Commands.UpdateUsersByAdmin
{
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, UserManager<User> userManager, IMapper mapper, IUserAccessor userAccessor)
        {
            _userAccessor = userAccessor;
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (user == null)
            {
                return Result<Unit>.Failure("User not found");
            }

            var currentRoles = await _userManager.GetRolesAsync(await _context.Users.FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUserName()));

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any(role => role is Roles.Owner or Roles.Admin) && !currentRoles.Any(role => role is Roles.Owner))
            {
                return Result<Unit>.Failure("You cannot update admins unless you are the owner");
            }

            _mapper.Map(request, user);

            if (!string.IsNullOrWhiteSpace(request.RemoveImage))
                user.Image = null;

            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    var errors = string.Join("; ", passwordResult.Errors.Select(e => e.Description));
                    return Result<Unit>.Failure($"Failed to reset password: {errors}");
                }
            }

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;
            return result ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("Failed. some of the properties like username, email and phone number might already exist.");
        }
    }
}
