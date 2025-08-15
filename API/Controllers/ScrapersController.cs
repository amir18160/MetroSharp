using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Queries = Application.Scrapers.Queries;

namespace API.Controllers
{
    [Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
    public class ScrapersController : BaseApiController
    {
        [HttpGet("get-latest-movies")]
        public async Task<IActionResult> GetLatestMovies([FromQuery] Queries.GetLatestTorrents.Query query)
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

        [HttpGet("search-yts")]
        public async Task<IActionResult> SearchYTSTorrents([FromQuery] Queries.SearchYTS.Query query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        [HttpGet("search-1337x")]
        public async Task<IActionResult> Search1337xTorrents([FromQuery] Queries.Search1337x.Query query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        [HttpGet("search-rarbg")]
        public async Task<IActionResult> SearchRarbgTorrents([FromQuery] Queries.SearchRarbg.Query query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        [HttpGet("search-subtitle")]
        public async Task<IActionResult> SearchSubtitle([FromQuery] Queries.SearchSubtitle.Query query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        [HttpPost("get-available-subtitles")]
        public async Task<IActionResult> GetAvailableSubtitles([FromBody] Queries.GetSubtitles.Query query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }
    }
}