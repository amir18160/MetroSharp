namespace Domain.Models.TelegramBot.ManuelMessages;

public class MovieSeriesNotificationParams
{
    public string ImdbId { get; set; }
    public UpdateDetails UpdateDetails { get; set; }
    public MovieSeriesNotificationType NotificationType { get; set; }
}