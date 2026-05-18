using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using SafeVault.Application.IServices;
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
        context.User = CreatePrincipal();

        var csrf = new Mock<ICsrfTokenService>();
        var middleware = new CsrfTokenMiddleware(_ => Task.CompletedTask, csrf.Object);

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
        var csrf = new Mock<ICsrfTokenService>();
        var middleware = new CsrfTokenMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        }, csrf.Object);

        await middleware.Invoke(context);

        Assert.True(called);
    }

    [Fact]
    public async Task Invoke_ShouldAllow_WhenTokenIsValid()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/vaults";
        context.Response.Body = new MemoryStream();
        context.User = CreatePrincipal();

        context.Request.Headers["X-CSRF-Token"] = "valid";

        var csrf = new Mock<ICsrfTokenService>();
        csrf.Setup(x => x.TryValidate("valid", It.IsAny<Guid>())).Returns(true);

        var called = false;
        var middleware = new CsrfTokenMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        }, csrf.Object);

        await middleware.Invoke(context);

        Assert.True(called);
    }

    private static ClaimsPrincipal CreatePrincipal()
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }, "test");
        return new ClaimsPrincipal(identity);
    }
}
