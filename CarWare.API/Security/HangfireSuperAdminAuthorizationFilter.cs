using CarWare.API.Security;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication;

namespace CarWare.API.Security;

public sealed class HangfireSuperAdminAuthorizationFilter
    : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        var result = httpContext
            .AuthenticateAsync(HangfireCookieDefaults.Scheme)
            .GetAwaiter()
            .GetResult();

        return result.Succeeded
            && result.Principal?.Identity?.IsAuthenticated == true
            && result.Principal.IsInRole("SUPERADMIN");
    }
}