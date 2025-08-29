using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


using Commands = Application.Tags.Commands;
using Queries = Application.Tags.Queries;

namespace API.Controllers
{
    [Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
    public class TagsController : BaseApiController
    {
        [HttpGet("query-tags")]
        public async Task<IActionResult> GetTags([FromQuery] Queries.GetTags.Query query)
        {
            return HandleResult(await Mediator.Send(query));
        }

        [HttpPost("create-tag")]
        public async Task<IActionResult> CreateTag([FromBody] Commands.Create.Command command)
        {
            return HandleResult(await Mediator.Send(command));
        }

        [HttpDelete("delete-tag/{id}")]
        public async Task<IActionResult> DeleteTag([FromRoute] Commands.Delete.Command command)
        {
            return HandleResult(await Mediator.Send(command));
        }
    }
}