using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Commands = Application.TorrentManager.Commands;

namespace API.Controllers
{
    [Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
    public class ProcessController : BaseApiController
    {
        [HttpPost("add-torrent-task")]
        public async Task<IActionResult> AddTorrentProcess([FromBody] Commands.AddTorrentProcess.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
    }
}