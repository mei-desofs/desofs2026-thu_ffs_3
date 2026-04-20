using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;

namespace SafeVault.DomainTests;

public class DomainEntityCoverageTests
{
    [Fact]
    public void User_AddRefreshToken_ShouldRevokeOldestWhenActiveLimitIsExceeded()
    {
        var user = new User("user@example.com", "hash", UserRole.Manager);
        var expiresAt = DateTime.UtcNow.AddHours(2);

        for (var i = 0; i < 6; i++)
        {
            user.AddRefreshToken($"token-{i}", expiresAt);
        }

        Assert.Equal(6, user.RefreshTokens.Count);
        Assert.Single(user.RefreshTokens, x => x.IsRevoked);
    }

    [Fact]
    public void User_RevokeRefreshToken_ShouldReturnFalse_WhenTokenDoesNotExist()
    {
        var user = new User("user@example.com", "hash", UserRole.Viewer);

        var result = user.RevokeRefreshToken("missing");

        Assert.False(result);
    }

    [Fact]
    public void Vault_GrantAccess_ShouldUpdateExistingEntry_WhenUserAlreadyExists()
    {
        var ownerId = Guid.NewGuid();
        var grantedUserId = Guid.NewGuid();
        var vault = new Vault("Main Vault", "desc", ownerId, "c:/vault", 30, false);

        vault.GrantAccess(grantedUserId, ownerId, AccessLevel.Read);
        vault.GrantAccess(grantedUserId, ownerId, AccessLevel.ReadWrite);

        Assert.Single(vault.Accesses);
        Assert.Equal(AccessLevel.ReadWrite, vault.Accesses.Single().AccessLevel);
    }

    [Fact]
    public void Vault_GrantAccess_ShouldThrow_WhenVaultIsArchived()
    {
        var ownerId = Guid.NewGuid();
        var vault = new Vault("Main Vault", "desc", ownerId, "c:/vault", 30, false);
        vault.Archive();

        Assert.Throws<InvalidOperationException>(() =>
            vault.GrantAccess(Guid.NewGuid(), ownerId, AccessLevel.Read));
    }

    [Fact]
    public void Document_AddVersion_ShouldThrow_WhenDocumentIsDeleted()
    {
        var doc = new Document(Guid.NewGuid(), Guid.NewGuid(), "a.txt", "a-v1.txt", "c:/a-v1.txt", "text/plain", 10,
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", DocumentClassification.Internal);

        doc.SoftDelete();

        Assert.Throws<InvalidOperationException>(() =>
            doc.AddVersion("a-v2.txt", "c:/a-v2.txt", "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb", Guid.NewGuid(), 11));
    }

    [Fact]
    public void Rehydrate_Factories_ShouldRestoreState()
    {
        var tokenId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var vaultId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var refreshToken = RefreshToken.Rehydrate(tokenId, userId, "hash", now.AddMinutes(-5), now.AddMinutes(30), true);
        var access = VaultAccess.Rehydrate(Guid.NewGuid(), vaultId, userId, Guid.NewGuid(), now, AccessLevel.Read);
        var version = DocumentVersion.Rehydrate(Guid.NewGuid(), docId, 2, "stored-v2", "sha", userId, 123, now);
        var audit = AuditLog.Rehydrate(Guid.NewGuid(), AuditEventType.Login, userId, docId, "Document", "127.0.0.1", "agent", now, true, "ok");

        Assert.True(refreshToken.IsRevoked);
        Assert.Equal(AccessLevel.Read, access.AccessLevel);
        Assert.Equal(2, version.VersionNumber);
        Assert.Equal(AuditEventType.Login, audit.EventType);
    }
}
