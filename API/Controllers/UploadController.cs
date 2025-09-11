using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Commands = Application.Uploads.Commands;

namespace API.Controllers;

[Authorize(Policy = RolesPolicy.OwnerOrAdmin)]
public class UploadController : BaseApiController
{
    [HttpPost("upload-zip-subtitle")]
    public async Task<IActionResult> UploadFromDirectSubtitle([FromForm] Commands.UploadSubtitleZip.Command command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}