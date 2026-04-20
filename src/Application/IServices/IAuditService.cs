using SafeVault.Application.DTOs.Audit;

namespace SafeVault.Application.IServices;

public interface IAuditService
{
    Task<IReadOnlyCollection<AuditLogDto>> SearchAsync(DateTime? fromUtc, DateTime? toUtc, Guid? userId, CancellationToken cancellationToken = default);
}
