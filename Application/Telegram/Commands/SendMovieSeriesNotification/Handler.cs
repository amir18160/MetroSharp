using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Telegram.Commands.SendMovieSeriesNotification
{
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly ITelegramManuelMessages _manuelMessages;
        private readonly ILogger<Handler> _logger;

        public Handler(ITelegramManuelMessages manuelMessages, ILogger<Handler> logger)
        {
            _manuelMessages = manuelMessages;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                await _manuelMessages.SendMovieSeriesNotificationPost(request.NotificationParams);
                
                return Result<Unit>.Success(Unit.Value);
            }
            catch (Exception e)
            {
                _logger.LogError("failed to send message to the channel. message: {error}", e.Message);
                return Result<Unit>.Failure("Failed to send message to the channel");
            }
        }
    }
}