using System.Net;

namespace SafeVault.InterfaceAdapters.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteProblemAsync(context, ex);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var statusCode = exception switch
        {
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = statusCode;

        var message = statusCode switch
        {
            StatusCodes.Status401Unauthorized => "Unauthorized.",
            StatusCodes.Status404NotFound => "Resource not found.",
            StatusCodes.Status400BadRequest => "Invalid request.",
            _ => "Internal server error."
        };

        var payload = new
        {
            error = message,
            statusCode = context.Response.StatusCode,
            correlationId = context.TraceIdentifier
        };

        await context.Response.WriteAsJsonAsync(payload);
    }
}
