using FluentValidation;

namespace Application.Indexer.Queries.SearchIndexer
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.SearchQuery)
                .NotEmpty();
        }
    }
}