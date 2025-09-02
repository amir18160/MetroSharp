using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Commands = Application.Telegram.Commands;

// using Queries = 

namespace API.Controllers
{
    public class TelegramController : BaseApiController
    {
        [Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
        [HttpPost("send-movie-series-notification")]
        public async Task<IActionResult> SendMovieSeriesNotificationPost(
            [FromBody] Commands.SendMovieSeriesNotification.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
    }
}