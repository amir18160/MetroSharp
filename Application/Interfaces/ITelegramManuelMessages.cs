using Domain.Models.TelegramBot.ManuelMessages;

namespace Application.Interfaces;

public interface ITelegramManuelMessages
{
    public Task SendMovieSeriesNotificationPost(MovieSeriesNotificationParams @params);
}