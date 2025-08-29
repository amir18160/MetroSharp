using FluentValidation;

namespace Application.Tags.Commands.Delete
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}