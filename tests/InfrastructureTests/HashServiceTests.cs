using SafeVault.Infrastructure.Security;

namespace SafeVault.InfrastructureTests;

public class HashServiceTests
{
    [Fact]
    public void ComputeSha256_ShouldReturnDeterministicHash()
    {
        var sut = new HashService();

        var first = sut.ComputeSha256("safevault");
        var second = sut.ComputeSha256("safevault");

        Assert.Equal(first, second);
        Assert.Equal(64, first.Length);
    }
}
