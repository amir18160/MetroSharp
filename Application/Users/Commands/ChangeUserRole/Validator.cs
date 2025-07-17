using Domain.Core;
using FluentValidation;

namespace Application.Users.Commands.ChangeUserRole
{

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.");

            RuleFor(x => x.NewRole)
                .Must(role => role == Roles.User || role == Roles.Admin)
                .WithMessage("Only 'User' or 'Admin' roles are allowed.");
        }
    }
}
