namespace SafeVault.Domain.EntityModels;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public bool IsRevoked { get; private set; }

    private RefreshToken()
    {
        TokenHash = string.Empty;
    }

    public RefreshToken(Guid userId, string tokenHash, DateTime expiresAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static RefreshToken Rehydrate(Guid id, Guid userId, string tokenHash, DateTime createdAtUtc, DateTime expiresAtUtc, bool isRevoked)
    {
        return new RefreshToken
        {
            Id = id,
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAtUtc = createdAtUtc,
            ExpiresAtUtc = expiresAtUtc,
            IsRevoked = isRevoked
        };
    }

    public void Revoke() => IsRevoked = true;
}
