using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

using Commands = Application.Plans.Commands;
using Queries = Application.Plans.Queries;


public class PlanController : BaseApiController
{
    [HttpGet("get-plans")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlans([FromQuery] Queries.QueryPlans.Query query)
    {
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }
    
    [HttpPost("new-plan")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> CreatePlan([FromBody] Commands.CreatePlan.Command command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
    
    [HttpPost("update-plan/{id}")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> UpdatePlan([FromBody] Commands.UpdatePlan.Command command, Guid id)
    {
        command.Id = id;
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}