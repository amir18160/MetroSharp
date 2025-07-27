using Application.Core;
using FluentValidation;

namespace Application.Tasks.Queries.GetTasks
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Title).MaximumLength(255);
            RuleFor(x => x.ImdbId).MaximumLength(20);
            RuleFor(x => x.TorrentHash).MaximumLength(40);
            RuleFor(x => x.UserId).MaximumLength(36);

            RuleFor(x => x.State).IsInEnum().When(x => x.State.HasValue);
            RuleFor(x => x.TaskType).IsInEnum().When(x => x.TaskType.HasValue);
            RuleFor(x => x.Priority).IsInEnum().When(x => x.Priority.HasValue);

            RuleFor(x => x.CreatedAt).SetValidator(new DateFilterValidator()).When(x => x.CreatedAt != null);
            RuleFor(x => x.UpdatedAt).SetValidator(new DateFilterValidator()).When(x => x.UpdatedAt != null);
            
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
