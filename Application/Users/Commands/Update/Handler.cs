using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Persistence;
using Domain.Entities;
using Application.Core;
using Application.Interfaces;

namespace Application.Users.Commands.Update
{

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IUserAccessor _userAccessor;
        private readonly IMapper _mapper;

        public Handler(DataContext context, UserManager<User> userManager, IUserAccessor userAccessor, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _userAccessor = userAccessor;
            _mapper = mapper;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(_userAccessor.GetUserName());

            if (user == null)
            {
                return Result<Unit>.Failure("User not found.");
            }

    
            _mapper.Map(request, user);


            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                if (string.IsNullOrEmpty(request.CurrentPassword))
                {
                    return Result<Unit>.Failure("Current password is required to change password.");
                }

    
                var passwordCheck = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
                if (!passwordCheck)
                {
                    return Result<Unit>.Failure("Invalid current password.");
                }

    
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (!changePasswordResult.Succeeded)
                {

                    var errors = string.Join(", ", changePasswordResult.Errors.Select(e => e.Description));
                    return Result<Unit>.Failure($"Failed to change password: {errors}");
                }
            }
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<Unit>.Failure($"Failed to update profile: {errors}");
            }

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
