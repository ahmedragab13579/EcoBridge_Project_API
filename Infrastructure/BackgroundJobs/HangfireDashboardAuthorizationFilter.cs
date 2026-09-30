using Hangfire.Dashboard;
using System.Security.Claims;

namespace EcoBridge.Infrastructure.BackgroundJobs;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (httpContext?.User?.Identity == null || !httpContext.User.Identity.IsAuthenticated)
        {
            return false;
        }

        // Restrict Hangfire Dashboard access exclusively to users with the Admin role
        return httpContext.User.IsInRole("Admin") ||
               httpContext.User.HasClaim(ClaimTypes.Role, "Admin");
    }
}
