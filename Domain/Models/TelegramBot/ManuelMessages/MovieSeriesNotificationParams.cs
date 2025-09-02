namespace Domain.Models.TelegramBot.ManuelMessages;

public class MovieSeriesNotificationParams
{
    public string ImdbId { get; set; }
    public UpdateDetails UpdateDetails { get; set; }
    public MovieSeriesNotificationType NotificationType { get; set; }
    public bool ShouldWriteReview { set; get; }
    public string AdminNote { get; set; }
}