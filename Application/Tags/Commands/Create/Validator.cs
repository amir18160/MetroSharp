using Domain.Enums;
using FluentValidation;

namespace Application.Tags.Commands.Create
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Type).IsInEnum();
            RuleFor(x => x.OmdbItemId).NotEmpty();
        }
    }
}