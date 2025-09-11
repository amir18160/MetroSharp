using Application.Core;
using Domain.Models.TelegramBot.ManuelMessages;
using MediatR;

namespace Application.Telegram.Commands.SendMovieSeriesNotification;

public class Command : IRequest<Result<Unit>>
{
    public MovieSeriesNotificationParams NotificationParams { get; set; }
}