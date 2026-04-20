using Moq;
using SafeVault.Application.DTOs.Documents;
using SafeVault.Application.IServices;
using SafeVault.Application.Services;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Domain.IRepositories;

namespace SafeVault.ApplicationTests;

public class DocumentServiceTests
{
    [Fact]
    public async Task Upload_ShouldThrow_WhenMimeTypeIsUnsupported()
    {
        var service = new DocumentService(
            Mock.Of<IVaultRepository>(),
            Mock.Of<IDocumentRepository>(),
            Mock.Of<IFileStorageService>(),
            Mock.Of<IHashService>(),
            Mock.Of<IAuditWriter>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadAsync(Guid.NewGuid(), new UploadDocumentRequest(Guid.NewGuid(), "x.exe", "application/x-msdownload", 10, DocumentClassification.Restricted, new MemoryStream(new byte[] { 1 }))));
    }

    [Fact]
    public async Task Download_ShouldThrow_WhenIntegrityCheckFails()
    {
        var actorId = Guid.NewGuid();
        var owner = actorId;
        var vault = new Vault("Vault", "desc", owner, "c:/tmp", 10, false);

        var document = new Document(vault.Id, actorId, "a.pdf", "stored.pdf", "c:/tmp/stored.pdf", "application/pdf", 10, new string('a', 64), DocumentClassification.Internal);

        var docRepo = new Mock<IDocumentRepository>();
        docRepo.Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var vaultRepo = new Mock<IVaultRepository>();
        vaultRepo.Setup(x => x.GetByIdAsync(vault.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vault);

        var fileStorage = new Mock<IFileStorageService>();
        fileStorage.Setup(x => x.ReadFileAsync(document.FilePath, It.IsAny<CancellationToken>())).ReturnsAsync(new MemoryStream(new byte[] { 1, 2, 3 }));

        var hash = new Mock<IHashService>();
        hash.Setup(x => x.ComputeSha256(It.IsAny<Stream>())).Returns(new string('b', 64));

        var service = new DocumentService(vaultRepo.Object, docRepo.Object, fileStorage.Object, hash.Object, Mock.Of<IAuditWriter>());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DownloadAsync(actorId, document.Id));
    }

    [Fact]
    public async Task SoftDelete_ShouldMarkDeletedAndRemoveFile()
    {
        var actorId = Guid.NewGuid();
        var vault = new Vault("Vault", "desc", actorId, "c:/tmp", 10, false);
        var document = new Document(vault.Id, actorId, "a.pdf", "stored.pdf", "c:/tmp/stored.pdf", "application/pdf", 10, new string('a', 64), DocumentClassification.Internal);

        var docRepo = new Mock<IDocumentRepository>();
        docRepo.Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var vaultRepo = new Mock<IVaultRepository>();
        vaultRepo.Setup(x => x.GetByIdAsync(vault.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vault);

        var fileStorage = new Mock<IFileStorageService>();
        var service = new DocumentService(vaultRepo.Object, docRepo.Object, fileStorage.Object, Mock.Of<IHashService>(), Mock.Of<IAuditWriter>());

        await service.SoftDeleteAsync(actorId, document.Id);

        Assert.True(document.IsDeleted);
        docRepo.Verify(x => x.UpdateAsync(document, It.IsAny<CancellationToken>()), Times.Once);
        fileStorage.Verify(x => x.DeleteFileAsync(document.FilePath, It.IsAny<CancellationToken>()), Times.Once);
    }
}
