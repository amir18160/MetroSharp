using FluentValidation;

namespace Application.Tasks.Commands.CancelTask
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}