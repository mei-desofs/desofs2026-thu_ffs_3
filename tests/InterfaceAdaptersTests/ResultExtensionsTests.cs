using System.Security.Claims;
using SafeVault.InterfaceAdapters;

namespace SafeVault.InterfaceAdaptersTests;

public class ResultExtensionsTests
{
    [Fact]
    public void GetRequiredUserId_ShouldReturnGuidFromSubClaim()
    {
        var userId = Guid.NewGuid();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("sub", userId.ToString())
        ], "test"));

        var parsed = principal.GetRequiredUserId();

        Assert.Equal(userId, parsed);
    }
}
