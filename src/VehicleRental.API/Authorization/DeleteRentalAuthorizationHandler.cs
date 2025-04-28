using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace VehicleRental.API.Authorization
{
    public class DeleteRentalAuthorizationHandler : AuthorizationHandler<DeleteRentalRequirement>
    {
        private readonly IConfiguration _configuration;

        public DeleteRentalAuthorizationHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DeleteRentalRequirement requirement)
        {
            var httpContext = context.Resource as HttpContext;
            if (httpContext != null)
            {
                if (httpContext.Request.Headers.TryGetValue("X-Admin-Token", out var token) && token == _configuration["AdminToken"])
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }

    public class DeleteRentalRequirement : IAuthorizationRequirement
    {
    }
}