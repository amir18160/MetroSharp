using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Queries = Application.TorrentManager.Queries;

namespace API.Controllers
{
    [Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
    public class TorrentManagerController : BaseApiController
    {
        [HttpGet("get-all-qbit-torrents")]
        public async Task<IActionResult> GetAllQbitTorrents(Queries.GetAllQbitTorrents.Query query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }
    }
}