using Microsoft.AspNetCore.Http;
using SafeVault.InterfaceAdapters.Middleware;

namespace SafeVault.InterfaceAdaptersTests;

public class CsrfTokenMiddlewareTests
{
    [Fact]
    public async Task Invoke_ShouldReturn400_WhenMutatingRequestWithoutToken()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/vaults";
        context.Response.Body = new MemoryStream();

        var middleware = new CsrfTokenMiddleware(_ => Task.CompletedTask);

        await middleware.Invoke(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_ShouldAllowAuthRouteWithoutToken()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/auth/login";
        context.Response.Body = new MemoryStream();

        var called = false;
        var middleware = new CsrfTokenMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.Invoke(context);

        Assert.True(called);
    }
}
