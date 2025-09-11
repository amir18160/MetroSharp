using Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Queries = Application.Documents.Queries;
using Commands = Application.Documents.Commands;

namespace API.Controllers;

public class DocumentsController : BaseApiController
{
    [HttpGet("get-temp-documents")]
    [Authorize(RolesPolicy.OwnerOrAdmin)]
    public async Task<IActionResult> GetTempDocuments([FromQuery] Queries.GetTemporaryDocuments.Query query)
    {
        var result = await Mediator.Send(query);
        return HandlePagedResult(result);
    }

    [HttpPost("update-temp-document")]
    [Authorize(RolesPolicy.OwnerOrAdmin)]
    public async Task<IActionResult> UpdateTempDocuments([FromBody] Commands.UpdateTemporaryDocument.Command command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}