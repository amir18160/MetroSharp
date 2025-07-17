using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Commands = Application.Users.Commands;
using Queries = Application.Users.Queries;

namespace API.Controllers
{
    public class UsersController : BaseApiController
    {
        /****************************/
        /****** User Section *******/
        /****************************/

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] Commands.Update.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
        [Authorize]
        [HttpPost("send-confirmation-email")]
        public async Task<IActionResult> SendConfirmationEmail(Commands.SendConfirmationEmail.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        [AllowAnonymous]
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(Commands.ConfirmEmail.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        [Authorize(Policy = RolesPolicy.Self)]
        [HttpPost("{Id}")]
        public async Task<IActionResult> UpdateSelf([FromBody] Commands.Update.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        
        [AllowAnonymous]
        [HttpPost("request-reset-password")]
        public async Task<IActionResult> RequestResetPassword([FromBody] Commands.RequestResetPassword.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        [AllowAnonymous]
        [HttpPost("confirm-reset-password")]
        public async Task<IActionResult> ConfirmResetPassword([FromBody] Commands.ResetPassword.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /****************************/
        /****** Admin Section *******/
        /****************************/

        [Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
        [HttpPost("active-deactive")]
        public async Task<IActionResult> ActiveDeactiveUser(Commands.ActiveDeactive.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        [Authorize(Roles = Roles.Owner)]
        [HttpPost("change-role")]
        public async Task<IActionResult> ChangeUserRole(Commands.ChangeUserRole.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        [Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
        [HttpGet("query-users")]
        public async Task<IActionResult> QueryUsers([FromQuery] Queries.GetUsers.Query query)
        {
            var result = await Mediator.Send(query);
            return HandlePagedResult(result);
        }

        [Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
        [HttpGet("get-unique-user")]
        public async Task<IActionResult> GetUserByUniqueIdentifier([FromQuery] Queries.GetSingleUser.Query query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        [Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
        [HttpPost("update-users")]
        public async Task<IActionResult> UpdateUserByAdmin([FromBody] Commands.UpdateUsersByAdmin.Command command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
    }
}
