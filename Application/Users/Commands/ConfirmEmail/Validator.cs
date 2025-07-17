using FluentValidation;

namespace Application.Users.Commands.ConfirmEmail
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required.");
        }
    }
}