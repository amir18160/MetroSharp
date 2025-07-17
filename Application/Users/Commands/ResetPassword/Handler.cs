using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Users.Commands.ResetPassword
{
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;
        private readonly UserManager<User> _userManager;

        public Handler(DataContext context, IUserAccessor userAccessor, UserManager<User> userManager)
        {
            _userManager = userManager;
            _userAccessor = userAccessor;
            _context = context;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUserName(), cancellationToken);

            if (user == null)
            {
                return Result<Unit>.Failure("user does not exist.");
            }

            var verification = _context.EmailVerifications
                .FirstOrDefault(x => x.UserId == user.Id && x.Purpose == EmailPurpose.PasswordReset && x.Expiration > DateTime.Now && !x.Used);

            if (verification == null)
            {
                return Result<Unit>.Failure("code has expired or already used.");
            }

            if (request.Code.ToString() != verification.Code)
            {
                return Result<Unit>.Failure("code is invalid.");
            }

            verification.Used = true;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
            if (!passwordResult.Succeeded)
            {
                var errors = string.Join("; ", passwordResult.Errors.Select(e => e.Description));
                return Result<Unit>.Failure($"Failed to reset password: {errors}");
            }

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;
            return result
                ? Result<Unit>.Success(Unit.Value)
                : Result<Unit>.Failure("failed to confirm confirmation code, try again later.");
        }
    }
}
