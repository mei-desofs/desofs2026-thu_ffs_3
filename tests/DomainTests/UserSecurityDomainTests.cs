using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;

namespace SafeVault.DomainTests;

public class UserSecurityDomainTests
{
    [Fact]
    public void AddRefreshToken_ShouldRevokeOldestWhenMoreThanFiveActive()
    {
        var user = new User("x@y.com", "hash", UserRole.Manager);
        for (var i = 0; i < 6; i++)
        {
            user.AddRefreshToken($"token-{i}", DateTime.UtcNow.AddHours(1));
        }

        Assert.Equal(6, user.RefreshTokens.Count);
        Assert.Equal(1, user.RefreshTokens.Count(x => x.IsRevoked));
    }

    [Fact]
    public void RevokeRefreshToken_ShouldReturnFalseWhenNotFound()
    {
        var user = new User("x@y.com", "hash", UserRole.Manager);
        var result = user.RevokeRefreshToken("missing");

        Assert.False(result);
    }
}
