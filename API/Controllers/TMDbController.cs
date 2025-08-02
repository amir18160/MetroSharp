using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Queries = Application.TMDb.Queries;

namespace API.Controllers
{
    [Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
    public class TMDbController : BaseApiController
    {
        [HttpGet("search-multi")]
        public async Task<IActionResult> SearchMulti([FromQuery] Queries.SearchMulti.Query query)
        {
            var res = await Mediator.Send(query);
            return HandleResult(res);
        }

        [HttpGet("get-movie-details")]
        public async Task<IActionResult> GetMovieDetails([FromQuery] Queries.GetMovieById.Query query)
        {
            var res = await Mediator.Send(query);
            return HandleResult(res);
        }

        [HttpGet("get-tv-show-details")]
        public async Task<IActionResult> GetTvShowDetails([FromQuery] Queries.GetTvShowById.Query query)
        {
            var res = await Mediator.Send(query);
            return HandleResult(res);
        }
    }
}