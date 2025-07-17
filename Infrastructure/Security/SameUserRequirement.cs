using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Infrastructure.Security
{
    public class SameUserRequirement : IAuthorizationRequirement { }
    public class SameUserHandler : AuthorizationHandler<SameUserRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SameUserHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameUserRequirement requirement)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var routeUserId = _httpContextAccessor.HttpContext?.Request.RouteValues["id"]?.ToString();

            if (userId != null && userId == routeUserId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
