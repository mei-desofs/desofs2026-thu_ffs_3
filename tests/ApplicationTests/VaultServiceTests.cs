using Moq;
using SafeVault.Application.DTOs.Vaults;
using SafeVault.Application.IServices;
using SafeVault.Application.Services;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.IRepositories;

namespace SafeVault.ApplicationTests;

public class VaultServiceTests
{
    [Fact]
    public async Task Create_ShouldCreateDirectoryAndPersistVault()
    {
        var ownerId = Guid.NewGuid();
        var repo = new Mock<IVaultRepository>();
        var storage = new Mock<IFileStorageService>();
        storage.Setup(x => x.CreateVaultDirectory(It.IsAny<Guid>(), "Ops Vault")).Returns("c:/storage/vault");

        var sut = new VaultService(repo.Object, storage.Object, Mock.Of<IAuditWriter>());

        var result = await sut.CreateAsync(ownerId, new CreateVaultRequest("Ops Vault", "desc", 15, false));

        Assert.Equal("Ops Vault", result.Name);
        repo.Verify(x => x.AddAsync(It.IsAny<Vault>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldThrow_WhenActorNotOwner()
    {
        var vault = new Vault("Vault", "desc", Guid.NewGuid(), "c:/v", 10, false);
        var repo = new Mock<IVaultRepository>();
        repo.Setup(x => x.GetByIdAsync(vault.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vault);

        var sut = new VaultService(repo.Object, Mock.Of<IFileStorageService>(), Mock.Of<IAuditWriter>());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.UpdateAsync(vault.Id, Guid.NewGuid(), new UpdateVaultRequest("Vault 2", "d", 10, false)));
    }
}
