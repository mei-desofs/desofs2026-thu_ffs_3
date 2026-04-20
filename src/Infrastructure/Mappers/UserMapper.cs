using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Infrastructure.DataModels;

namespace SafeVault.Infrastructure.Mappers;

public static class UserMapper
{
    public static User ToDomain(UserDataModel model, IEnumerable<RefreshTokenDataModel> refreshTokenModels)
    {
        var refreshTokens = refreshTokenModels.Select(ToDomain).ToArray();

        return User.Rehydrate(
            model.Id,
            model.Email,
            model.PasswordHash,
            Enum.Parse<UserRole>(model.Role),
            model.IsActive,
            model.FailedLoginAttempts,
            model.LockoutUntilUtc,
            model.CreatedAtUtc,
            model.LastLoginAtUtc,
            refreshTokens);
    }

    public static UserDataModel ToDataModel(User user)
        => new()
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            FailedLoginAttempts = user.FailedLoginAttempts,
            LockoutUntilUtc = user.LockoutUntilUtc,
            CreatedAtUtc = user.CreatedAtUtc,
            LastLoginAtUtc = user.LastLoginAtUtc
        };

    public static RefreshTokenDataModel ToDataModel(RefreshToken token)
        => new()
        {
            Id = token.Id,
            UserId = token.UserId,
            TokenHash = token.TokenHash,
            CreatedAtUtc = token.CreatedAtUtc,
            ExpiresAtUtc = token.ExpiresAtUtc,
            IsRevoked = token.IsRevoked
        };

    public static RefreshToken ToDomain(RefreshTokenDataModel model)
        => RefreshToken.Rehydrate(model.Id, model.UserId, model.TokenHash, model.CreatedAtUtc, model.ExpiresAtUtc, model.IsRevoked);
}
