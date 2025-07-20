using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


using Queries = Application.Scrapers.Queries;
using Commands = Application.Scrapers.Commands;

namespace API.Controllers
{
    [Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
    public class ScrapersController : BaseApiController
    {
        [HttpGet("get-latest-movies")]
        public async Task<IActionResult> GetLatestMovies(Queries.GetLatestTorrents.Query query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }
        
        [HttpGet("get-all-yts-movies")]
        public async Task<IActionResult> GetAllYtsMovies([FromQuery] Queries.GetAllYTSMovies.Query query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }


        /***********************************
        ********* Torrent Details  *********
        ************************************/

        [HttpPost("get-torrent-details")]
        public async Task<IActionResult> GetYtsDetails([FromBody] Queries.GetTorrentDetails.Query query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /***********************************
        ********* Torrent Search  *********
        ************************************/

        [HttpPost("search-yts")]
        public async Task<IActionResult> SearchYTSTorrents([FromBody] Commands.SearchYTS.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        [HttpPost("search-1337x")]
        public async Task<IActionResult> Search1337xTorrents([FromBody] Commands.Search1337x.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        [HttpPost("search-rarbg")]
        public async Task<IActionResult> SearchRarbgTorrents([FromBody] Commands.SearchRarbg.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }



    }
}