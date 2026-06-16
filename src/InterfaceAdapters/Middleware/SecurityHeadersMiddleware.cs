namespace SafeVault.InterfaceAdapters.Middleware;

public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        context.Response.Headers.TryAdd("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
        context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
        context.Response.Headers.TryAdd("Content-Security-Policy", "default-src 'none'; frame-ancestors 'none';");
        context.Response.Headers.TryAdd("Cross-Origin-Resource-Policy", "same-origin");
        context.Response.Headers.TryAdd("Cache-Control", "no-store, no-cache, must-revalidate");

        await next(context);
    }
}
