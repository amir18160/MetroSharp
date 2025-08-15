using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Queries = Application.Indexer.Queries;

namespace API.Controllers
{
    [Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
    public class IndexerController : BaseApiController
    {
        [HttpGet("get-indexers-list")]
        public async Task<IActionResult> GetIndexers([FromQuery] Queries.GetIndexerProviders.Query query)
        {
            var res = await Mediator.Send(query);
            return HandleResult(res);
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] Queries.SearchIndexer.Query query)
        {
            var res = await Mediator.Send(query);
            return HandleResult(res);
        }
    }
}