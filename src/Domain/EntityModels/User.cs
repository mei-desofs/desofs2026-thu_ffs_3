using SafeVault.Domain.Enums;
using SafeVault.Domain.ValueObjects;

namespace SafeVault.Domain.EntityModels;

public class User
{
    private readonly List<RefreshToken> _refreshTokens = [];

    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutUntilUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
    }

    public User(string email, string passwordHash, UserRole role)
    {
        Id = Guid.NewGuid();
        Email = new Email(email).Value;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static User Rehydrate(
        Guid id,
        string email,
        string passwordHash,
        UserRole role,
        bool isActive,
        int failedLoginAttempts,
        DateTime? lockoutUntilUtc,
        DateTime createdAtUtc,
        DateTime? lastLoginAtUtc,
        IEnumerable<RefreshToken>? refreshTokens = null)
    {
        var user = new User
        {
            Id = id,
            Email = email,
            PasswordHash = passwordHash,
            Role = role,
            IsActive = isActive,
            FailedLoginAttempts = failedLoginAttempts,
            LockoutUntilUtc = lockoutUntilUtc,
            CreatedAtUtc = createdAtUtc,
            LastLoginAtUtc = lastLoginAtUtc
        };

        if (refreshTokens is not null)
        {
            user._refreshTokens.Clear();
            user._refreshTokens.AddRange(refreshTokens);
        }

        return user;
    }

    public void Update(string email, UserRole role)
    {
        Email = new Email(email).Value;
        Role = role;
    }

    public void UpdatePasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    public bool IsLocked() => LockoutUntilUtc.HasValue && LockoutUntilUtc.Value > DateTime.UtcNow;

    public void RegisterFailedLoginAttempt()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
        {
            LockoutUntilUtc = DateTime.UtcNow.AddMinutes(15);
            FailedLoginAttempts = 0;
        }
    }

    public void RegisterSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockoutUntilUtc = null;
        LastLoginAtUtc = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;

    public void AddRefreshToken(string tokenHash, DateTime expiresAtUtc)
    {
        var activeCount = _refreshTokens.Count(x => !x.IsRevoked && x.ExpiresAtUtc > DateTime.UtcNow);
        if (activeCount >= 5)
        {
            var oldest = _refreshTokens
                .Where(x => !x.IsRevoked && x.ExpiresAtUtc > DateTime.UtcNow)
                .OrderBy(x => x.CreatedAtUtc)
                .First();
            oldest.Revoke();
        }

        _refreshTokens.Add(new RefreshToken(Id, tokenHash, expiresAtUtc));
    }

    public bool RevokeRefreshToken(string tokenHash)
    {
        var token = _refreshTokens.FirstOrDefault(x => x.TokenHash == tokenHash && !x.IsRevoked);
        if (token is null)
        {
            return false;
        }

        token.Revoke();
        return true;
    }
}
