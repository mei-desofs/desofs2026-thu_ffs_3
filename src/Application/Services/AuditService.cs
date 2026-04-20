using SafeVault.Application.DTOs.Audit;
using SafeVault.Application.IServices;
using SafeVault.Domain.IRepositories;

namespace SafeVault.Application.Services;

public class AuditService(IAuditLogRepository repository) : IAuditService
{
    public async Task<IReadOnlyCollection<AuditLogDto>> SearchAsync(DateTime? fromUtc, DateTime? toUtc, Guid? userId, CancellationToken cancellationToken = default)
    {
        var logs = await repository.SearchAsync(fromUtc, toUtc, userId, cancellationToken);
        return logs
            .OrderByDescending(x => x.TimestampUtc)
            .Select(x => new AuditLogDto(x.Id, x.EventType, x.UserId, x.TargetResourceId, x.TargetResourceType, x.IpAddress, x.UserAgent, x.TimestampUtc, x.Success, x.Details))
            .ToArray();
    }
}
