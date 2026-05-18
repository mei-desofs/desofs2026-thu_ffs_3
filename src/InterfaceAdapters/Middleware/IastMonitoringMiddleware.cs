using System.Text.RegularExpressions;
using SafeVault.Application.IServices;
using SafeVault.Domain.Enums;
using SafeVault.InterfaceAdapters;

namespace SafeVault.InterfaceAdapters.Middleware;

public class IastMonitoringMiddleware(RequestDelegate next, ILogger<IastMonitoringMiddleware> logger, IAuditWriter auditWriter, IConfiguration configuration)
{
    private static readonly Regex[] SuspiciousPatterns =
    [
        new Regex(@"(?i)(\bor\b\s+\d+\s*=\s*\d+)", RegexOptions.Compiled),
        new Regex(@"(?i)(union\s+select|select\s+.+\s+from)", RegexOptions.Compiled),
        new Regex(@"(?i)(<script|javascript:)", RegexOptions.Compiled),
        new Regex(@"(\.\./|\.\\)", RegexOptions.Compiled)
    ];

    private readonly bool _enabled = configuration.GetValue("Iast:Enabled", false);

    public async Task Invoke(HttpContext context)
    {
        if (_enabled)
        {
            var inputs = CollectInputs(context);
            foreach (var value in inputs)
            {
                if (IsSuspicious(value))
                {
                    var userId = context.User?.Identity?.IsAuthenticated == true
                        ? context.User.GetRequiredUserId()
                        : (Guid?)null;

                    await auditWriter.WriteAsync(
                        AuditEventType.SecurityAlert,
                        userId,
                        null,
                        "Request",
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        context.Request.Headers.UserAgent.ToString(),
                        false,
                        "Suspicious input pattern detected by IAST middleware.",
                        context.RequestAborted);

                    logger.LogWarning("IAST suspicious input detected for request {Path}", context.Request.Path);
                    break;
                }
            }
        }

        await next(context);
    }

    private static IEnumerable<string> CollectInputs(HttpContext context)
    {
        foreach (var pair in context.Request.Query)
        {
            foreach (var item in pair.Value)
            {
                yield return item ?? string.Empty;
            }
        }

        foreach (var value in context.Request.RouteValues.Values)
        {
            if (value is not null)
            {
                yield return value.ToString() ?? string.Empty;
            }
        }
    }

    private static bool IsSuspicious(string input)
        => SuspiciousPatterns.Any(pattern => pattern.IsMatch(input));
}
