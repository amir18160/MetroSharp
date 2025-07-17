using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Commands = Application.BannedUsers.Commands;
using Queries = Application.BannedUsers.Queries;

namespace API.Controllers
{
    [Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
    public class BanController : BaseApiController
    {
        [HttpPost("ban-user")]
        public async Task<IActionResult> BanUser(Commands.BanUser.Command command)
        {
            return HandleResult(await Mediator.Send(command));
        }

        [HttpPost("unban-user")]
        public async Task<IActionResult> UnbanUser(Commands.UnbanUser.Command command)
        {
            return HandleResult(await Mediator.Send(command));
        }

        [HttpGet("query-banned-users")]
        public async Task<IActionResult> QueryBannedUsers([FromQuery]Queries.GetBannedUsers.Query query)
        {
            return HandleResult(await Mediator.Send(query));
        }
    }
}