using FluentValidation;

namespace Application.Tasks.Commands.RetryTask
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}