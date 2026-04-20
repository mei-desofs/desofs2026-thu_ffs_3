using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Infrastructure.DataModels;

namespace SafeVault.Infrastructure.Mappers;

public static class VaultMapper
{
    public static Vault ToDomain(VaultDataModel model, IEnumerable<VaultAccessDataModel> accessModels)
    {
        var accesses = accessModels.Select(ToDomain).ToArray();

        return Vault.Rehydrate(
            model.Id,
            model.Name,
            model.Description,
            model.OwnerId,
            model.DirectoryPath,
            model.RetentionDays,
            model.AutoDeleteOnExpiry,
            model.IsArchived,
            model.CreatedAtUtc,
            accesses);
    }

    public static VaultDataModel ToDataModel(Vault vault)
        => new()
        {
            Id = vault.Id,
            Name = vault.Name,
            Description = vault.Description,
            OwnerId = vault.OwnerId,
            DirectoryPath = vault.DirectoryPath,
            RetentionDays = vault.RetentionDays,
            AutoDeleteOnExpiry = vault.AutoDeleteOnExpiry,
            IsArchived = vault.IsArchived,
            CreatedAtUtc = vault.CreatedAtUtc
        };

    public static VaultAccessDataModel ToDataModel(VaultAccess access)
        => new()
        {
            Id = access.Id,
            VaultId = access.VaultId,
            UserId = access.UserId,
            GrantedBy = access.GrantedBy,
            GrantedAtUtc = access.GrantedAtUtc,
            AccessLevel = access.AccessLevel.ToString()
        };

    public static VaultAccess ToDomain(VaultAccessDataModel model)
        => VaultAccess.Rehydrate(
            model.Id,
            model.VaultId,
            model.UserId,
            model.GrantedBy,
            model.GrantedAtUtc,
            Enum.Parse<AccessLevel>(model.AccessLevel));
}
