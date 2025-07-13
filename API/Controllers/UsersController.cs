using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Update = Application.Users.Update;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] Update.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
      
        [HttpPut]
        public async Task<IActionResult> DeleteUser([FromBody] Update.Command command)
        {
            return BadRequest();
        }
    }
}
