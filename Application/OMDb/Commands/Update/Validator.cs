using FluentValidation;

namespace Application.OMDb.Commands.Update
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.OmdbItem).NotNull();
        }
    }
}