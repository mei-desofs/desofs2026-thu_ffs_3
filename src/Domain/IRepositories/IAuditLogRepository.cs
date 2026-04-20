using SafeVault.Domain.EntityModels;

namespace SafeVault.Domain.IRepositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AuditLog>> SearchAsync(DateTime? fromUtc, DateTime? toUtc, Guid? userId, CancellationToken cancellationToken = default);
}
