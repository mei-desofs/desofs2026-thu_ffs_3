using SafeVault.Application.DTOs.Vaults;
using SafeVault.Application.IServices;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Domain.IRepositories;

namespace SafeVault.Application.Services;

public class VaultService(
    IVaultRepository vaultRepository,
    IFileStorageService fileStorageService,
    IAuditWriter auditWriter) : IVaultService
{
    public async Task<VaultDto> CreateAsync(Guid ownerId, CreateVaultRequest request, CancellationToken cancellationToken = default)
    {
        var vault = new Vault(request.Name, request.Description, ownerId, string.Empty, request.RetentionDays, request.AutoDeleteOnExpiry);
        var directoryPath = fileStorageService.CreateVaultDirectory(vault.Id, request.Name);
        vault.SetDirectoryPath(directoryPath);

        await vaultRepository.AddAsync(vault, cancellationToken);

        await auditWriter.WriteAsync(AuditEventType.VaultCreated, ownerId, vault.Id, nameof(Vault), "n/a", "n/a", true, "Vault created.", cancellationToken);

        return Map(vault);
    }

    public async Task<IReadOnlyCollection<VaultDto>> GetAccessibleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var vaults = await vaultRepository.GetAccessibleByUserAsync(userId, cancellationToken);
        return vaults.Select(Map).ToArray();
    }

    public async Task<VaultDto> UpdateAsync(Guid vaultId, Guid actorId, UpdateVaultRequest request, CancellationToken cancellationToken = default)
    {
        var vault = await vaultRepository.GetByIdAsync(vaultId, cancellationToken)
            ?? throw new KeyNotFoundException("Vault not found.");

        EnsureOwnership(vault, actorId);
        vault.UpdateDetails(request.Name, request.Description, request.RetentionDays, request.AutoDeleteOnExpiry);
        await vaultRepository.UpdateAsync(vault, cancellationToken);

        await auditWriter.WriteAsync(AuditEventType.VaultUpdated, actorId, vault.Id, nameof(Vault), "n/a", "n/a", true, "Vault updated.", cancellationToken);
        return Map(vault);
    }

    public async Task ArchiveAsync(Guid vaultId, Guid actorId, CancellationToken cancellationToken = default)
    {
        var vault = await vaultRepository.GetByIdAsync(vaultId, cancellationToken)
            ?? throw new KeyNotFoundException("Vault not found.");

        EnsureOwnership(vault, actorId);
        vault.Archive();

        await vaultRepository.UpdateAsync(vault, cancellationToken);
        await auditWriter.WriteAsync(AuditEventType.VaultArchived, actorId, vault.Id, nameof(Vault), "n/a", "n/a", true, "Vault archived.", cancellationToken);
    }

    public async Task GrantAccessAsync(Guid vaultId, Guid actorId, GrantVaultAccessRequest request, CancellationToken cancellationToken = default)
    {
        var vault = await vaultRepository.GetByIdAsync(vaultId, cancellationToken)
            ?? throw new KeyNotFoundException("Vault not found.");

        EnsureOwnership(vault, actorId);
        vault.GrantAccess(request.UserId, actorId, request.AccessLevel);

        await vaultRepository.UpdateAsync(vault, cancellationToken);
        await auditWriter.WriteAsync(AuditEventType.VaultAccessGranted, actorId, vault.Id, nameof(Vault), "n/a", "n/a", true, $"Access granted to {request.UserId}.", cancellationToken);
    }

    public async Task RevokeAccessAsync(Guid vaultId, Guid actorId, Guid userId, CancellationToken cancellationToken = default)
    {
        var vault = await vaultRepository.GetByIdAsync(vaultId, cancellationToken)
            ?? throw new KeyNotFoundException("Vault not found.");

        EnsureOwnership(vault, actorId);
        vault.RevokeAccess(userId);

        await vaultRepository.UpdateAsync(vault, cancellationToken);
        await auditWriter.WriteAsync(AuditEventType.VaultAccessRevoked, actorId, vault.Id, nameof(Vault), "n/a", "n/a", true, $"Access revoked from {userId}.", cancellationToken);
    }

    private static void EnsureOwnership(Vault vault, Guid actorId)
    {
        if (vault.OwnerId != actorId)
        {
            throw new UnauthorizedAccessException("Only owner can perform this operation.");
        }
    }

    private static VaultDto Map(Vault vault)
        => new(vault.Id, vault.Name, vault.Description, vault.OwnerId, vault.RetentionDays, vault.AutoDeleteOnExpiry, vault.IsArchived, vault.CreatedAtUtc);
}
