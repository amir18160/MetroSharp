using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Commands = Application.Tasks.Commands;
using Queries = Application.Tasks.Queries;

namespace API.Controllers
{
    [Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
    public class TasksController : BaseApiController
    {
        [HttpPost("add-torrent-task")]
        public async Task<IActionResult> AddTorrentProcess([FromBody] Commands.AddTorrentTask.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        [HttpGet("query-tasks")]
        public async Task<IActionResult> GetTasks([FromQuery] Queries.GetTasks.Query query)
        {
            var result = await Mediator.Send(query);
            return HandlePagedResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask([FromRoute] Queries.GetSingleTask.Query query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        [HttpDelete("cancel-task/{id}")]
        public async Task<IActionResult> CancelTask([FromRoute] Commands.CancelTask.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
    }
}