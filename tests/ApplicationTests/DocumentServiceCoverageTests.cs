using Moq;
using SafeVault.Application.DTOs.Documents;
using SafeVault.Application.IServices;
using SafeVault.Application.Services;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Domain.IRepositories;

namespace SafeVault.ApplicationTests;

public class DocumentServiceCoverageTests
{
    [Fact]
    public async Task Upload_ShouldThrow_WhenMimeTypeIsNotAllowed()
    {
        var sut = new DocumentService(Mock.Of<IVaultRepository>(), Mock.Of<IDocumentRepository>(), Mock.Of<IFileStorageService>(), Mock.Of<IHashService>(), Mock.Of<IAuditWriter>());

        using var stream = new MemoryStream([1, 2, 3]);
        var request = new UploadDocumentRequest(Guid.NewGuid(), "malware.exe", "application/x-msdownload", stream.Length, DocumentClassification.Internal, stream);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.UploadAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task Upload_ShouldPersistDocument_WhenActorHasWriteAccess()
    {
        var actorId = Guid.NewGuid();
        var vault = new Vault("Main Vault", "desc", actorId, "c:/vault", 30, false);

        var vaultRepository = new Mock<IVaultRepository>();
        vaultRepository.Setup(x => x.GetByIdAsync(vault.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vault);

        var documentRepository = new Mock<IDocumentRepository>();

        var storage = new Mock<IFileStorageService>();
        storage.Setup(x => x.SaveFileAsync(vault.Id, "doc.pdf", It.IsAny<Stream>(), It.IsAny<CancellationToken>())).ReturnsAsync("c:/vault/doc-stored.pdf");

        var hashService = new Mock<IHashService>();
        hashService.Setup(x => x.ComputeSha256(It.IsAny<Stream>())).Returns("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef");

        var sut = new DocumentService(vaultRepository.Object, documentRepository.Object, storage.Object, hashService.Object, Mock.Of<IAuditWriter>());

        using var stream = new MemoryStream([10, 20, 30]);
        var request = new UploadDocumentRequest(vault.Id, "doc.pdf", "application/pdf", stream.Length, DocumentClassification.Internal, stream);

        var result = await sut.UploadAsync(actorId, request);

        Assert.Equal("doc.pdf", result.OriginalFileName);
        documentRepository.Verify(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Download_ShouldThrow_WhenIntegrityCheckFails()
    {
        var actorId = Guid.NewGuid();
        var vault = new Vault("Main Vault", "desc", actorId, "c:/vault", 30, false);

        var document = new Document(vault.Id, actorId, "doc.txt", "stored.txt", "c:/vault/stored.txt", "text/plain", 5,
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", DocumentClassification.Public);

        var vaultRepository = new Mock<IVaultRepository>();
        vaultRepository.Setup(x => x.GetByIdAsync(vault.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vault);

        var documentRepository = new Mock<IDocumentRepository>();
        documentRepository.Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var storage = new Mock<IFileStorageService>();
        storage.Setup(x => x.ReadFileAsync(document.FilePath, It.IsAny<CancellationToken>())).ReturnsAsync(new MemoryStream([1, 2, 3]));

        var hashService = new Mock<IHashService>();
        hashService.Setup(x => x.ComputeSha256(It.IsAny<Stream>())).Returns("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");

        var sut = new DocumentService(vaultRepository.Object, documentRepository.Object, storage.Object, hashService.Object, Mock.Of<IAuditWriter>());

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.DownloadAsync(actorId, document.Id));
    }

    [Fact]
    public async Task SoftDelete_ShouldMarkAndDeletePhysicalFile_WhenActorHasWriteAccess()
    {
        var actorId = Guid.NewGuid();
        var vault = new Vault("Main Vault", "desc", actorId, "c:/vault", 30, false);

        var document = new Document(vault.Id, actorId, "doc.txt", "stored.txt", "c:/vault/stored.txt", "text/plain", 5,
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", DocumentClassification.Internal);

        var vaultRepository = new Mock<IVaultRepository>();
        vaultRepository.Setup(x => x.GetByIdAsync(vault.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vault);

        var documentRepository = new Mock<IDocumentRepository>();
        documentRepository.Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var storage = new Mock<IFileStorageService>();

        var sut = new DocumentService(vaultRepository.Object, documentRepository.Object, storage.Object, Mock.Of<IHashService>(), Mock.Of<IAuditWriter>());

        await sut.SoftDeleteAsync(actorId, document.Id);

        Assert.True(document.IsDeleted);
        documentRepository.Verify(x => x.UpdateAsync(document, It.IsAny<CancellationToken>()), Times.Once);
        storage.Verify(x => x.DeleteFileAsync(document.FilePath, It.IsAny<CancellationToken>()), Times.Once);
    }
}
