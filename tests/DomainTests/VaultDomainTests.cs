using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;

namespace SafeVault.DomainTests;

public class VaultDomainTests
{
    [Fact]
    public void GrantAccess_ShouldAddAndUpdateAccessLevel()
    {
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var vault = new Vault("Team Vault", "docs", ownerId, "c:/tmp", 30, false);

        vault.GrantAccess(userId, ownerId, AccessLevel.Read);
        vault.GrantAccess(userId, ownerId, AccessLevel.ReadWrite);

        Assert.Single(vault.Accesses);
        Assert.Equal(AccessLevel.ReadWrite, vault.Accesses.First().AccessLevel);
    }

    [Fact]
    public void GrantAccess_ShouldThrowWhenArchived()
    {
        var vault = new Vault("Archive", "docs", Guid.NewGuid(), "c:/tmp", 30, false);
        vault.Archive();

        Assert.Throws<InvalidOperationException>(() => vault.GrantAccess(Guid.NewGuid(), Guid.NewGuid(), AccessLevel.Read));
    }

    [Fact]
    public void SetDirectoryPath_ShouldThrowOnEmptyPath()
    {
        var vault = new Vault("Folder", "docs", Guid.NewGuid(), "c:/tmp", 30, false);
        Assert.Throws<ArgumentException>(() => vault.SetDirectoryPath(string.Empty));
    }
}
