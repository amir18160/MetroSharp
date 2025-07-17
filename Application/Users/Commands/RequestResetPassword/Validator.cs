using FluentValidation;

namespace Application.Users.Commands.RequestResetPassword
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }
}