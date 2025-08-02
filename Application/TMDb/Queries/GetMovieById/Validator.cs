using FluentValidation;

namespace Application.TMDb.Queries.GetMovieById
{
    public class Validator: AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}