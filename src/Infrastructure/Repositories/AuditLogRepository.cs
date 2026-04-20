using Microsoft.EntityFrameworkCore;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.IRepositories;
using SafeVault.Infrastructure.Mappers;

namespace SafeVault.Infrastructure.Repositories;

public class AuditLogRepository(SafeVaultDbContext dbContext) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog log, CancellationToken cancellationToken = default)
    {
        await dbContext.AuditLogs.AddAsync(AuditLogMapper.ToDataModel(log), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AuditLog>> SearchAsync(DateTime? fromUtc, DateTime? toUtc, Guid? userId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.AuditLogs.AsQueryable();

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.TimestampUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.TimestampUtc <= toUtc.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        var models = await query
            .OrderByDescending(x => x.TimestampUtc)
            .Take(1000)
            .ToArrayAsync(cancellationToken);

        return models.Select(AuditLogMapper.ToDomain).ToArray();
    }
}
