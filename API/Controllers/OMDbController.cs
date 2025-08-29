using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Queries = Application.OMDb.Queries;
using Commands = Application.OMDb.Commands;

namespace API.Controllers
{
    [Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
    public class OmdbController : BaseApiController
    {
        [HttpGet("query-omdb")]
        public async Task<IActionResult> QueryOMDbItems([FromQuery] Queries.QueryOMDbItems.Query query)
        {
            return HandlePagedResult(await Mediator.Send(query));
        }

        [Authorize(Roles = Roles.Owner)]
        [HttpPut("update-omdb/{id}")]
        public async Task<IActionResult> UpdateOMDbItem(string id, [FromBody] Commands.Update.Command command)
        {
            command.Id = id;
            return HandleResult(await Mediator.Send(command));
        }
    }
}