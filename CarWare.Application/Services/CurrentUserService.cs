using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId =>
        _httpContextAccessor.HttpContext?
        .User?
        .FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public int? ServiceCenterId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst("ServiceCenterId")?.Value;

            return int.TryParse(value, out var id)
                ? id
                : null;
        }
    }
}