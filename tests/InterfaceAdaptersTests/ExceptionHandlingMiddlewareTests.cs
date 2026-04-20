using Microsoft.AspNetCore.Http;
using SafeVault.InterfaceAdapters.Middleware;

namespace SafeVault.InterfaceAdaptersTests;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task Invoke_ShouldMapInvalidOperationTo400()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<ExceptionHandlingMiddleware>();
        var middleware = new ExceptionHandlingMiddleware(_ => throw new InvalidOperationException("invalid"), logger);

        await middleware.Invoke(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }
}
