using Microsoft.Extensions.Logging;
using SafeVault.Application.IServices;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Domain.IRepositories;

namespace SafeVault.Infrastructure.Storage;

public class AuditWriterService(IAuditLogRepository repository, ILogger<AuditWriterService> logger) : IAuditWriter
{
    public async Task WriteAsync(
        AuditEventType eventType,
        Guid? userId,
        Guid? targetResourceId,
        string targetResourceType,
        string ipAddress,
        string userAgent,
        bool success,
        string details,
        CancellationToken cancellationToken = default)
    {
        var log = new AuditLog(eventType, userId, targetResourceId, targetResourceType, ipAddress, userAgent, success, details);
        await repository.AddAsync(log, cancellationToken);

        logger.LogInformation("Audit {EventType} User={UserId} Success={Success} Target={TargetType}:{TargetId}", eventType, userId, success, targetResourceType, targetResourceId);
    }
}
