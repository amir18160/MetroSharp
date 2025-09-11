using Domain.Models.TelegramBot.ManuelMessages;
using FluentValidation;

namespace Application.Telegram.Commands.SendMovieSeriesNotification;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.NotificationParams)
            .NotNull()
            .WithMessage("Notification parameters are required.")
            .SetValidator(new MovieSeriesNotificationParamsValidator());
    }
}

public class MovieSeriesNotificationParamsValidator : AbstractValidator<MovieSeriesNotificationParams>
{
    public MovieSeriesNotificationParamsValidator()
    {
        RuleFor(x => x.ImdbId)
            .NotEmpty()
            .WithMessage("IMDb ID is required.")
            .Matches(@"^tt\d{6,12}$")
            .WithMessage("IMDb ID must be in a valid format (e.g., tt1234567).");

        RuleFor(x => x.NotificationType)
            .IsInEnum()
            .WithMessage("Invalid notification type.");

        RuleFor(x => x.UpdateDetails)
            .NotNull()
            .When(x => x.NotificationType == MovieSeriesNotificationType.UpdatedSeries)
            .WithMessage("Update details are required for UpdatedSeries notifications.")
            .SetValidator(new UpdateDetailsValidator())
            .When(x => x.NotificationType == MovieSeriesNotificationType.UpdatedSeries);
    }
}

public class UpdateDetailsValidator : AbstractValidator<UpdateDetails>
{
    public UpdateDetailsValidator()
    {
        RuleFor(x => x.Season)
            .GreaterThan(0)
            .WithMessage("Season must be greater than 0.");

        RuleFor(x => x.Episode)
            .GreaterThan(0)
            .When(x => !x.IsFullSeason)
            .WithMessage("Episode must be greater than 0 unless it's a full season.");

        RuleFor(x => x.IsFullSeason)
            .NotNull()
            .WithMessage("IsFullSeason flag must be specified.");
    }
}