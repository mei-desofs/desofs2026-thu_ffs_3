using SafeVault.Domain.Enums;

namespace SafeVault.Domain.EntityModels;

public class VaultAccess
{
    public Guid Id { get; private set; }
    public Guid VaultId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid GrantedBy { get; private set; }
    public DateTime GrantedAtUtc { get; private set; }
    public AccessLevel AccessLevel { get; private set; }

    private VaultAccess()
    {
    }

    public VaultAccess(Guid vaultId, Guid userId, Guid grantedBy, AccessLevel accessLevel)
    {
        Id = Guid.NewGuid();
        VaultId = vaultId;
        UserId = userId;
        GrantedBy = grantedBy;
        GrantedAtUtc = DateTime.UtcNow;
        AccessLevel = accessLevel;
    }

    public static VaultAccess Rehydrate(Guid id, Guid vaultId, Guid userId, Guid grantedBy, DateTime grantedAtUtc, AccessLevel accessLevel)
    {
        return new VaultAccess
        {
            Id = id,
            VaultId = vaultId,
            UserId = userId,
            GrantedBy = grantedBy,
            GrantedAtUtc = grantedAtUtc,
            AccessLevel = accessLevel
        };
    }

    public void UpdateLevel(AccessLevel accessLevel) => AccessLevel = accessLevel;
}
