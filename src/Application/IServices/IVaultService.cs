using SafeVault.Application.DTOs.Vaults;

namespace SafeVault.Application.IServices;

public interface IVaultService
{
    Task<VaultDto> CreateAsync(Guid ownerId, CreateVaultRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<VaultDto>> GetAccessibleAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<VaultDto> UpdateAsync(Guid vaultId, Guid actorId, UpdateVaultRequest request, CancellationToken cancellationToken = default);
    Task ArchiveAsync(Guid vaultId, Guid actorId, CancellationToken cancellationToken = default);
    Task GrantAccessAsync(Guid vaultId, Guid actorId, GrantVaultAccessRequest request, CancellationToken cancellationToken = default);
    Task RevokeAccessAsync(Guid vaultId, Guid actorId, Guid userId, CancellationToken cancellationToken = default);
}
