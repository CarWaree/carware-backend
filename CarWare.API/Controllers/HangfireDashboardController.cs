using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarWare.API.Security;

namespace CarWare.API.Controllers;

[ApiController]
[Route("api/admin/hangfire")]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "SUPERADMIN")]
public sealed class HangfireDashboardController : ControllerBase
{
    [HttpPost("session")]
    public async Task<IActionResult> CreateSession()
    {
        var sourceIdentity = User.Identities.FirstOrDefault();

        var identity = new ClaimsIdentity(
            User.Claims,
            HangfireCookieDefaults.Scheme,
            sourceIdentity?.NameClaimType ?? ClaimTypes.Name,
            sourceIdentity?.RoleClaimType ?? ClaimTypes.Role);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            HangfireCookieDefaults.Scheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(15)
            });

        return Ok(new
        {
            message = "Hangfire session created.",
            dashboardUrl = "/hangfire"
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            HangfireCookieDefaults.Scheme);

        return NoContent();
    }
}