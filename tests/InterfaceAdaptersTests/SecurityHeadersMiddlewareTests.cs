using Microsoft.AspNetCore.Http;
using SafeVault.InterfaceAdapters.Middleware;

namespace SafeVault.InterfaceAdaptersTests;

public class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task Invoke_ShouldAddSecurityHeaders()
    {
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        await middleware.Invoke(context);

        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"].ToString());
        Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"].ToString());
        Assert.Contains("default-src 'none'", context.Response.Headers["Content-Security-Policy"].ToString());
    }
}
