using FluentValidation;

namespace Application.TMDb.Queries.SearchMulti
{
    public class Validator: AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.SearchQuery)
                .NotEmpty();
        }
    }
}