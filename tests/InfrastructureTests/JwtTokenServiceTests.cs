using Microsoft.Extensions.Options;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Infrastructure.Options;
using SafeVault.Infrastructure.Security;

namespace SafeVault.InfrastructureTests;

public class JwtTokenServiceTests
{
    [Fact]
    public void GenerateAccessToken_ShouldReturnTokenAndExpiry()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "issuer",
            Audience = "aud",
            SigningKey = "12345678901234567890123456789012",
            AccessTokenMinutes = 60
        });

        var sut = new JwtTokenService(options);
        var user = new User("user@example.com", "hash", UserRole.Manager);

        var result = sut.GenerateAccessToken(user);

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.True(result.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64()
    {
        var options = Options.Create(new JwtOptions { SigningKey = "12345678901234567890123456789012" });
        var sut = new JwtTokenService(options);

        var token = sut.GenerateRefreshToken();

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(token.Length >= 80);
    }
}
