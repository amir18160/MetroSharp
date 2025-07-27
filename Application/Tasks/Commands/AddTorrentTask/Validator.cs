using FluentValidation;

namespace Application.Tasks.Commands.AddTorrentTask
{
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {   
            RuleFor(x => x.Title)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(255);

            RuleFor(x => x.Magnet)
                .NotEmpty()
                .Must(s => s.StartsWith("magnet:"))
                .WithMessage("A magnet must start with 'magnet:'");

            RuleFor(x => x.SubtitleUrl)
                .Must(s => s.StartsWith("http"))
                .When(s => !string.IsNullOrWhiteSpace(s?.SubtitleUrl))
                .WithMessage("SubtitleUrl must start with 'http' if provided.");

            RuleFor(x => x.ImdbId)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(20);

            RuleFor(x => x.SeasonNumber)
                .InclusiveBetween(1, 99)
                .When(x => x.SeasonNumber > 0)
                .WithMessage("SeasonNumber must be between 1 and 99 when provided.");

            RuleFor(x => x.EpisodeNumber)
                .InclusiveBetween(1, 999)
                .When(x => x.EpisodeNumber > 0)
                .WithMessage("EpisodeNumber must be between 1 and 999 when provided.");

            RuleFor(x => x.TaskType)
                .IsInEnum()
                .WithMessage("Invalid torrent task type.");

            RuleFor(x => x.Priority)
                .IsInEnum()
                .WithMessage("Invalid torrent task priority.");
        }
    }
}
