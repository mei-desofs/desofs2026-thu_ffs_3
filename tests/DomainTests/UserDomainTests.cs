using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Domain.ValueObjects;

namespace SafeVault.DomainTests;

public class UserDomainTests
{
    [Fact]
    public void PasswordPolicy_ShouldRejectWeakPassword()
    {
        var action = () => PasswordPolicy.Validate("weak");
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void User_ShouldLockAfterFiveFailedAttempts()
    {
        var user = new User("test@example.com", "hash", UserRole.Viewer);
        for (var i = 0; i < 5; i++)
        {
            user.RegisterFailedLoginAttempt();
        }

        Assert.True(user.IsLocked());
    }
}
