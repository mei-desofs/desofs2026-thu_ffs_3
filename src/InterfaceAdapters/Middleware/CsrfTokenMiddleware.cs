using SafeVault.Application.IServices;
using SafeVault.InterfaceAdapters;

namespace SafeVault.InterfaceAdapters.Middleware;

public class CsrfTokenMiddleware(RequestDelegate next, ICsrfTokenService csrfTokenService)
{
    public async Task Invoke(HttpContext context)
    {
        var method = context.Request.Method;
        var isMutating = HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);
        var isAuthRoute = context.Request.Path.StartsWithSegments("/api/auth", StringComparison.OrdinalIgnoreCase);

        if (isMutating && !isAuthRoute)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Unauthorized." });
                return;
            }

            if (!context.Request.Headers.TryGetValue("X-CSRF-Token", out var headerValue))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "Missing CSRF token." });
                return;
            }

            var userId = context.User.GetRequiredUserId();
            if (!csrfTokenService.TryValidate(headerValue.ToString(), userId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid CSRF token." });
                return;
            }
        }

        await next(context);
    }
}
