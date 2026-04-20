using System.Security.Claims;

namespace SafeVault.InterfaceAdapters;

public static class ResultExtensions
{
    public static Guid GetRequiredUserId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user context.");
        }

        return userId;
    }
}
