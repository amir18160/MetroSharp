using FluentValidation;

namespace Application.Scrapers.Commands.Search1337x
{
    public class Validator: AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.SearchTerm).NotEmpty().MaximumLength(100);
        }
    }
}