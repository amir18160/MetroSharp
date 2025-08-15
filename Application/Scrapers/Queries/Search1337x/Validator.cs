using FluentValidation;

namespace Application.Scrapers.Queries.Search1337x
{
    public class Validator: AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.SearchTerm).NotEmpty().MaximumLength(100);
        }
    }
}