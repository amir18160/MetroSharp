using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Commands = Application.Subscriptions.Commands;
using Queries = Application.Subscriptions.Queries;

namespace API.Controllers;

[Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
public class SubscriptionController : BaseApiController
{
    [HttpGet("get-subscriptions")]
    [Authorize(policy: RolesPolicy.OwnerOrAdmin)]
    public async Task<IActionResult> QuerySubscriptions([FromQuery] Queries.QuerySubscriptions.Query query)
    {
        var result = await Mediator.Send(query);
        return HandlePagedResult(result);
    }

    [HttpPost("new-subscription")]
    [Authorize(policy: RolesPolicy.OwnerOrAdmin)]
    public async Task<IActionResult> CreateNewSubscription(Commands.AddSubscription.Command command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("stats")]
    [Authorize(policy: RolesPolicy.OwnerOrAdmin)]
    public async Task<IActionResult> CreateNewSubscription([FromQuery] Queries.SubscriptionsStats.Query query)
    {
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }
    
    [HttpPost("extend-subscriptions")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> ExtendSubscriptions([FromBody] Commands.ExtendSubscriptions.Command command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}