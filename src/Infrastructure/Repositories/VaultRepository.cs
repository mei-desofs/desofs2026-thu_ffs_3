using Microsoft.EntityFrameworkCore;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.IRepositories;
using SafeVault.Infrastructure.Mappers;

namespace SafeVault.Infrastructure.Repositories;

public class VaultRepository(SafeVaultDbContext dbContext) : IVaultRepository
{
    public async Task<Vault?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vaultModel = await dbContext.Vaults.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (vaultModel is null)
        {
            return null;
        }

        var accessModels = await dbContext.VaultAccesses.AsNoTracking().Where(x => x.VaultId == id).ToArrayAsync(cancellationToken);
        return VaultMapper.ToDomain(vaultModel, accessModels);
    }

    public async Task<IReadOnlyCollection<Vault>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var vaultModels = await dbContext.Vaults.AsNoTracking().Where(x => x.OwnerId == ownerId).ToArrayAsync(cancellationToken);
        var accessModels = await dbContext.VaultAccesses.AsNoTracking().Where(x => vaultModels.Select(v => v.Id).Contains(x.VaultId)).ToArrayAsync(cancellationToken);

        return vaultModels.Select(v => VaultMapper.ToDomain(v, accessModels.Where(a => a.VaultId == v.Id))).ToArray();
    }

    public async Task<IReadOnlyCollection<Vault>> GetAccessibleByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var accessVaultIds = await dbContext.VaultAccesses
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.VaultId)
            .ToArrayAsync(cancellationToken);

        var vaultModels = await dbContext.Vaults
            .AsNoTracking()
            .Where(x => x.OwnerId == userId || accessVaultIds.Contains(x.Id))
            .ToArrayAsync(cancellationToken);

        var accessModels = await dbContext.VaultAccesses.AsNoTracking().Where(x => vaultModels.Select(v => v.Id).Contains(x.VaultId)).ToArrayAsync(cancellationToken);
        return vaultModels.Select(v => VaultMapper.ToDomain(v, accessModels.Where(a => a.VaultId == v.Id))).ToArray();
    }

    public async Task AddAsync(Vault vault, CancellationToken cancellationToken = default)
    {
        await dbContext.Vaults.AddAsync(VaultMapper.ToDataModel(vault), cancellationToken);
        foreach (var access in vault.Accesses)
        {
            await dbContext.VaultAccesses.AddAsync(VaultMapper.ToDataModel(access), cancellationToken);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Vault vault, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Vaults.FirstOrDefaultAsync(x => x.Id == vault.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Vault not found.");

        existing.Name = vault.Name;
        existing.Description = vault.Description;
        existing.DirectoryPath = vault.DirectoryPath;
        existing.RetentionDays = vault.RetentionDays;
        existing.AutoDeleteOnExpiry = vault.AutoDeleteOnExpiry;
        existing.IsArchived = vault.IsArchived;

        var existingAccesses = await dbContext.VaultAccesses.Where(x => x.VaultId == vault.Id).ToArrayAsync(cancellationToken);
        dbContext.VaultAccesses.RemoveRange(existingAccesses);
        foreach (var access in vault.Accesses)
        {
            await dbContext.VaultAccesses.AddAsync(VaultMapper.ToDataModel(access), cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Vault vault, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Vaults.FirstOrDefaultAsync(x => x.Id == vault.Id, cancellationToken);
        if (existing is null)
        {
            return;
        }

        var existingAccesses = await dbContext.VaultAccesses.Where(x => x.VaultId == vault.Id).ToArrayAsync(cancellationToken);
        dbContext.VaultAccesses.RemoveRange(existingAccesses);
        dbContext.Vaults.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
