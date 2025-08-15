using FluentValidation;

namespace Application.Scrapers.Queries.SearchRarbg
{
    public class Validator: AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.SearchTerm).NotEmpty().MaximumLength(100);
        }
    }
}