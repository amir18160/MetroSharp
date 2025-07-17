using FluentValidation;

namespace Application.BannedUsers.Commands.UnbanUser
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("There must be a UserId to Unban.");
        }
    }
}