using Hangfire.Dashboard;

namespace CarWare.API.Security;

public sealed class HangfireSuperAdminAuthorizationFilter
    : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("SUPERADMIN");
    }
}