using FluentValidation;

namespace Application.OMDb.Queries.QueryOMDbItems;

public class Validator : AbstractValidator<Query>
{
    public Validator()
    {
        RuleFor(x => x.ImdbId)
            .Must(x=> x.StartsWith("tt"))
            .WithMessage("Imdb id must start with 'tt'")
            .When(x => !string.IsNullOrWhiteSpace(x.ImdbId));

        RuleFor(x => x.TagType)
            .IsInEnum()
            .WithMessage("Tag type must be in enum range")
            .When(x => x.TagType.HasValue);
    }
}