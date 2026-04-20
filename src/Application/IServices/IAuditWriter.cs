using SafeVault.Domain.Enums;

namespace SafeVault.Application.IServices;

public interface IAuditWriter
{
    Task WriteAsync(
        AuditEventType eventType,
        Guid? userId,
        Guid? targetResourceId,
        string targetResourceType,
        string ipAddress,
        string userAgent,
        bool success,
        string details,
        CancellationToken cancellationToken = default);
}
