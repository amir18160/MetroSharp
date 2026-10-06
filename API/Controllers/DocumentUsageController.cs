using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Queries = Application.DocumentUsages.Queries;

namespace API.Controllers;

[Authorize]
public class DocumentUsageController : BaseApiController
{
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(Queries.GetGeneralUsageStats.Query query)
    {
        var res = await Mediator.Send(query);
        return HandleResult(res);
    }

    [HttpGet("user/{Id}")]
    public async Task<IActionResult> GetUserStats(Queries.GetUserUsageStats.Query query, string userId)
    {
        query.UserId = userId;
        var res = await Mediator.Send(query);
        return HandleResult(res);
    }

    [HttpGet("user-remaining/{Id}")]
    public async Task<IActionResult> GetUserRemaining(Queries.GetUserRemainingUsages.Query query, string userId)
    {
        query.UserId = userId;
        var res = await Mediator.Send(query);
        return HandleResult(res);
    }
}