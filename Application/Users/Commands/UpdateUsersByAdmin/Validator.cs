using FluentValidation;

namespace Application.Users.Commands.UpdateUsersByAdmin
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Username)
                .MinimumLength(3).When(x => !string.IsNullOrWhiteSpace(x.Username));

            RuleFor(x => x.NewPassword)
                .MinimumLength(6).When(x => !string.IsNullOrWhiteSpace(x.NewPassword))
                .WithMessage("Password must be at least 6 character and include UpperCase, LowerCase and a number.");
        }
    }
}
