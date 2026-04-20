using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Infrastructure;
using SafeVault.Infrastructure.DataModels;
using SafeVault.Infrastructure.Repositories;
using SafeVault.Infrastructure.Storage;

namespace SafeVault.InfrastructureTests;

public class RepositoryCoverageTests
{
    [Fact]
    public async Task VaultRepository_GetAccessibleByUser_ShouldIncludeOwnedAndShared()
    {
        await using var db = CreateDbContext();

        var ownerId = Guid.NewGuid();
        var sharedUser = Guid.NewGuid();

        var ownedVault = new VaultDataModel
        {
            Id = Guid.NewGuid(),
            Name = "Owned",
            Description = "desc",
            OwnerId = sharedUser,
            DirectoryPath = "c:/owned",
            RetentionDays = 30,
            AutoDeleteOnExpiry = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        var sharedVault = new VaultDataModel
        {
            Id = Guid.NewGuid(),
            Name = "Shared",
            Description = "desc",
            OwnerId = ownerId,
            DirectoryPath = "c:/shared",
            RetentionDays = 30,
            AutoDeleteOnExpiry = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Vaults.AddRange(ownedVault, sharedVault);
        db.VaultAccesses.Add(new VaultAccessDataModel
        {
            Id = Guid.NewGuid(),
            VaultId = sharedVault.Id,
            UserId = sharedUser,
            GrantedBy = ownerId,
            GrantedAtUtc = DateTime.UtcNow,
            AccessLevel = AccessLevel.Read.ToString()
        });
        await db.SaveChangesAsync();

        var repo = new VaultRepository(db);
        var accessible = await repo.GetAccessibleByUserAsync(sharedUser);

        Assert.Equal(2, accessible.Count);
    }

    [Fact]
    public async Task DocumentRepository_Update_ShouldPersistLatestVersionAndSoftDeleteFlags()
    {
        await using var db = CreateDbContext();
        var repo = new DocumentRepository(db);

        var document = new Document(Guid.NewGuid(), Guid.NewGuid(), "doc.txt", "doc-v1.txt", "c:/doc-v1.txt", "text/plain", 10,
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", DocumentClassification.Internal);
        await repo.AddAsync(document);

        document.AddVersion("doc-v2.txt", "c:/doc-v2.txt", "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb", Guid.NewGuid(), 12);
        document.SoftDelete();
        await repo.UpdateAsync(document);

        var loaded = await repo.GetByIdAsync(document.Id);

        Assert.NotNull(loaded);
        Assert.True(loaded!.IsDeleted);
        Assert.Equal(2, loaded.Versions.Count);
    }

    [Fact]
    public async Task AuditLogRepository_Search_ShouldFilterByDateAndUser()
    {
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();

        db.AuditLogs.AddRange(
            new AuditLogDataModel
            {
                Id = Guid.NewGuid(),
                EventType = AuditEventType.Login.ToString(),
                UserId = userId,
                TargetResourceType = "User",
                IpAddress = "127.0.0.1",
                UserAgent = "agent",
                Success = true,
                Details = "ok",
                TimestampUtc = DateTime.UtcNow.AddHours(-1)
            },
            new AuditLogDataModel
            {
                Id = Guid.NewGuid(),
                EventType = AuditEventType.Login.ToString(),
                UserId = Guid.NewGuid(),
                TargetResourceType = "User",
                IpAddress = "127.0.0.1",
                UserAgent = "agent",
                Success = true,
                Details = "ok",
                TimestampUtc = DateTime.UtcNow.AddHours(-3)
            });
        await db.SaveChangesAsync();

        var repo = new AuditLogRepository(db);
        var logs = await repo.SearchAsync(DateTime.UtcNow.AddHours(-2), DateTime.UtcNow, userId);

        Assert.Single(logs);
        Assert.Equal(userId, logs.Single().UserId);
    }

    [Fact]
    public async Task UserRepository_Delete_ShouldRemoveUserAndTokens()
    {
        await using var db = CreateDbContext();
        var repo = new UserRepository(db);

        var user = new User("delete@test.com", "hash", UserRole.Viewer);
        user.AddRefreshToken("token-hash", DateTime.UtcNow.AddHours(1));
        await repo.AddAsync(user);

        await repo.DeleteAsync(user);

        var loaded = await repo.GetByIdAsync(user.Id);
        Assert.Null(loaded);
        Assert.Empty(db.RefreshTokens.Where(x => x.UserId == user.Id));
    }

    [Fact]
    public async Task AuditWriterService_Write_ShouldPersistAuditEntry()
    {
        await using var db = CreateDbContext();
        var repository = new AuditLogRepository(db);
        var writer = new AuditWriterService(repository, NullLogger<AuditWriterService>.Instance);

        await writer.WriteAsync(AuditEventType.UserUpdated, Guid.NewGuid(), Guid.NewGuid(), "User", "127.0.0.1", "agent", true, "updated");

        Assert.Single(db.AuditLogs);
    }

    private static SafeVaultDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SafeVaultDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SafeVaultDbContext(options);
    }
}
