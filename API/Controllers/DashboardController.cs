using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


// using Commands = Application.Dashboard.Commands   
using Queries = Application.Dashboard.Queries;

namespace API.Controllers
{

    [Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
    public class DashboardController : BaseApiController
    {
        [HttpGet("system-info")]
        public async Task<IActionResult> GetSystemInfo([FromQuery] Queries.GetSystemInfo.Query query)
        {
            var res = await Mediator.Send(query);
            return HandleResult(res);
        }
    }
}
