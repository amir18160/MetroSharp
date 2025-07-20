using FluentValidation;

namespace Application.Scrapers.Commands.SearchRarbg
{
    public class Validator: AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.SearchTerm).NotEmpty().MaximumLength(100);
        }
    }
}