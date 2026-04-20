namespace SafeVault.InterfaceAdapters.Middleware;

public class CsrfTokenMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var method = context.Request.Method;
        var isMutating = HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);
        var isAuthRoute = context.Request.Path.StartsWithSegments("/api/auth", StringComparison.OrdinalIgnoreCase);

        if (isMutating && !isAuthRoute)
        {
            if (!context.Request.Headers.TryGetValue("X-CSRF-Token", out var headerValue) || headerValue != "safevault")
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "Missing or invalid CSRF token." });
                return;
            }
        }

        await next(context);
    }
}
