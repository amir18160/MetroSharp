using API.Common;
using Application.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        private IMediator _mediator;
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();

        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result == null)
                return NotFound(ApiResponse<T>.Error("Resource not found."));

            if (result.IsSuccess)
            {
                if (result.Value == null)
                    return NotFound(ApiResponse<T>.Error("No content."));

                return Ok(ApiResponse<T>.Success(result.Value));
            }

            return BadRequest(ApiResponse<T>.Error(result.Error));
        }

        protected IActionResult HandlePagedResult<T>(Result<PagedList<T>> result)
        {
            if (result == null)
                return NotFound(ApiResponse<object>.Error("Resource not found."));

            if (result.IsSuccess)
            {
                if (result.Value == null)
                    return NotFound(ApiResponse<object>.Error("No content."));

                var pagedData = new
                {
                    items = result.Value.ToList(),
                    currentPage = result.Value.CurrentPage,
                    pageSize = result.Value.PageSize,
                    totalPages = result.Value.TotalPages,
                    totalCount = result.Value.TotalCount
                };

                return Ok(ApiResponse<object>.Success(pagedData));
            }

            return BadRequest(ApiResponse<object>.Error(result.Error));
        }
    }
}
