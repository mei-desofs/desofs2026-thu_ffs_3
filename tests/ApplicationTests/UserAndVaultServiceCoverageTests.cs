using Moq;
using SafeVault.Application.DTOs.Users;
using SafeVault.Application.DTOs.Vaults;
using SafeVault.Application.IServices;
using SafeVault.Application.Services;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Domain.IRepositories;

namespace SafeVault.ApplicationTests;

public class UserAndVaultServiceCoverageTests
{
    [Fact]
    public async Task UserService_Create_ShouldPersistAndAudit()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByEmailAsync("create@example.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(x => x.Hash("ValidPassword!123")).Returns("hash");

        var auditWriter = new Mock<IAuditWriter>();

        var sut = new UserService(users.Object, passwordHasher.Object, auditWriter.Object);

        var result = await sut.CreateAsync(new CreateUserRequest("create@example.com", "ValidPassword!123", UserRole.Viewer));

        Assert.Equal("create@example.com", result.Email);
        users.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        auditWriter.Verify(x => x.WriteAsync(AuditEventType.UserCreated, It.IsAny<Guid?>(), It.IsAny<Guid?>(), nameof(User), "n/a", "n/a", true, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UserService_Update_ShouldDeactivate_WhenIsActiveIsFalse()
    {
        var user = new User("old@example.com", "hash", UserRole.Viewer);

        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var sut = new UserService(users.Object, Mock.Of<IPasswordHasher>(), Mock.Of<IAuditWriter>());

        var result = await sut.UpdateAsync(user.Id, new UpdateUserRequest("new@example.com", UserRole.Manager, false));

        Assert.Equal("new@example.com", result.Email);
        Assert.False(result.IsActive);
        users.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UserService_Delete_ShouldThrow_WhenUserMissing()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var sut = new UserService(users.Object, Mock.Of<IPasswordHasher>(), Mock.Of<IAuditWriter>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task VaultService_Update_ShouldThrow_WhenActorIsNotOwner()
    {
        var ownerId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var vault = new Vault("Main Vault", "desc", ownerId, "c:/vault", 30, false);

        var vaultRepository = new Mock<IVaultRepository>();
        vaultRepository.Setup(x => x.GetByIdAsync(vault.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vault);

        var sut = new VaultService(vaultRepository.Object, Mock.Of<IFileStorageService>(), Mock.Of<IAuditWriter>());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.UpdateAsync(vault.Id, actorId, new UpdateVaultRequest("Renamed Vault", "updated", 60, true)));
    }

    [Fact]
    public async Task VaultService_GrantAndRevoke_ShouldPersistUpdates()
    {
        var ownerId = Guid.NewGuid();
        var grantedUserId = Guid.NewGuid();
        var vault = new Vault("Main Vault", "desc", ownerId, "c:/vault", 30, false);

        var vaultRepository = new Mock<IVaultRepository>();
        vaultRepository.Setup(x => x.GetByIdAsync(vault.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vault);

        var sut = new VaultService(vaultRepository.Object, Mock.Of<IFileStorageService>(), Mock.Of<IAuditWriter>());

        await sut.GrantAccessAsync(vault.Id, ownerId, new GrantVaultAccessRequest(grantedUserId, AccessLevel.ReadWrite));
        Assert.True(vault.CanWrite(grantedUserId));

        await sut.RevokeAccessAsync(vault.Id, ownerId, grantedUserId);
        Assert.False(vault.CanRead(grantedUserId));

        vaultRepository.Verify(x => x.UpdateAsync(vault, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task VaultService_Create_ShouldSetDirectoryAndPersist()
    {
        var ownerId = Guid.NewGuid();
        var vaultRepository = new Mock<IVaultRepository>();
        var storage = new Mock<IFileStorageService>();
        storage.Setup(x => x.CreateVaultDirectory(It.IsAny<Guid>(), "Main Vault")).Returns("c:/data/main-vault");

        var sut = new VaultService(vaultRepository.Object, storage.Object, Mock.Of<IAuditWriter>());

        var result = await sut.CreateAsync(ownerId, new CreateVaultRequest("Main Vault", "desc", 45, true));

        Assert.Equal("Main Vault", result.Name);
        vaultRepository.Verify(x => x.AddAsync(It.IsAny<Vault>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
