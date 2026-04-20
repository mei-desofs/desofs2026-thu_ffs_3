using SafeVault.Domain.EntityModels;

namespace SafeVault.Domain.IRepositories;

public interface IVaultRepository
{
    Task<Vault?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Vault>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Vault>> GetAccessibleByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Vault vault, CancellationToken cancellationToken = default);
    Task UpdateAsync(Vault vault, CancellationToken cancellationToken = default);
    Task DeleteAsync(Vault vault, CancellationToken cancellationToken = default);
}
