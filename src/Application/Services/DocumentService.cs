using SafeVault.Application.DTOs.Documents;
using SafeVault.Application.IServices;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Domain.IRepositories;

namespace SafeVault.Application.Services;

public class DocumentService(
    IVaultRepository vaultRepository,
    IDocumentRepository documentRepository,
    IFileStorageService fileStorageService,
    IHashService hashService,
    IAuditWriter auditWriter) : IDocumentService
{
    public async Task<DocumentDto> UploadAsync(Guid actorId, UploadDocumentRequest request, CancellationToken cancellationToken = default)
    {
        EnsureUploadRules(request);

        var vault = await vaultRepository.GetByIdAsync(request.VaultId, cancellationToken)
            ?? throw new KeyNotFoundException("Vault not found.");

        if (!vault.CanWrite(actorId))
        {
            throw new UnauthorizedAccessException("No write access to vault.");
        }

        var storedPath = await fileStorageService.SaveFileAsync(request.VaultId, request.OriginalFileName, request.Content, cancellationToken);
        request.Content.Position = 0;
        var sha = hashService.ComputeSha256(request.Content);

        var storedFileName = Path.GetFileName(storedPath);
        var document = new Document(request.VaultId, actorId, request.OriginalFileName, storedFileName, storedPath, request.MimeType, request.FileSize, sha, request.Classification);

        await documentRepository.AddAsync(document, cancellationToken);

        await auditWriter.WriteAsync(AuditEventType.DocumentUploaded, actorId, document.Id, nameof(Document), "n/a", "n/a", true, "Document uploaded.", cancellationToken);

        return Map(document);
    }

    public async Task<DocumentDto> UploadNewVersionAsync(Guid actorId, Guid documentId, UploadDocumentRequest request, CancellationToken cancellationToken = default)
    {
        EnsureUploadRules(request);

        var document = await documentRepository.GetByIdAsync(documentId, cancellationToken)
            ?? throw new KeyNotFoundException("Document not found.");

        var vault = await vaultRepository.GetByIdAsync(document.VaultId, cancellationToken)
            ?? throw new KeyNotFoundException("Vault not found.");

        if (!vault.CanWrite(actorId))
        {
            throw new UnauthorizedAccessException("No write access to vault.");
        }

        var storedPath = await fileStorageService.SaveFileAsync(document.VaultId, request.OriginalFileName, request.Content, cancellationToken);
        request.Content.Position = 0;
        var sha = hashService.ComputeSha256(request.Content);

        document.AddVersion(Path.GetFileName(storedPath), storedPath, sha, actorId, request.FileSize);
        await documentRepository.UpdateAsync(document, cancellationToken);

        await auditWriter.WriteAsync(AuditEventType.DocumentUploaded, actorId, document.Id, nameof(Document), "n/a", "n/a", true, "Document version uploaded.", cancellationToken);
        return Map(document);
    }

    public async Task<DownloadDocumentResponse> DownloadAsync(Guid actorId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await documentRepository.GetByIdAsync(documentId, cancellationToken)
            ?? throw new KeyNotFoundException("Document not found.");

        if (document.IsDeleted)
        {
            throw new InvalidOperationException("Document is deleted.");
        }

        var vault = await vaultRepository.GetByIdAsync(document.VaultId, cancellationToken)
            ?? throw new KeyNotFoundException("Vault not found.");

        if (!vault.CanRead(actorId))
        {
            throw new UnauthorizedAccessException("No read access to vault.");
        }

        var stream = await fileStorageService.ReadFileAsync(document.FilePath, cancellationToken);
        var currentHash = hashService.ComputeSha256(stream);
        stream.Position = 0;

        if (!string.Equals(currentHash, document.Sha256Hash, StringComparison.OrdinalIgnoreCase))
        {
            await auditWriter.WriteAsync(AuditEventType.IntegrityFailure, actorId, document.Id, nameof(Document), "n/a", "n/a", false, "Document hash mismatch.", cancellationToken);
            throw new InvalidOperationException("Integrity check failed.");
        }

        await auditWriter.WriteAsync(AuditEventType.DocumentDownloaded, actorId, document.Id, nameof(Document), "n/a", "n/a", true, "Document downloaded.", cancellationToken);
        return new DownloadDocumentResponse(document.OriginalFileName, document.MimeType, stream);
    }

    public async Task SoftDeleteAsync(Guid actorId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await documentRepository.GetByIdAsync(documentId, cancellationToken)
            ?? throw new KeyNotFoundException("Document not found.");

        var vault = await vaultRepository.GetByIdAsync(document.VaultId, cancellationToken)
            ?? throw new KeyNotFoundException("Vault not found.");

        if (!vault.CanWrite(actorId))
        {
            throw new UnauthorizedAccessException("No write access to vault.");
        }

        document.SoftDelete();
        await documentRepository.UpdateAsync(document, cancellationToken);

        await fileStorageService.DeleteFileAsync(document.FilePath, cancellationToken);
        await auditWriter.WriteAsync(AuditEventType.DocumentDeleted, actorId, document.Id, nameof(Document), "n/a", "n/a", true, "Document deleted (soft delete + file removal).", cancellationToken);
    }

    public async Task<IReadOnlyCollection<DocumentDto>> ListByVaultAsync(Guid actorId, Guid vaultId, CancellationToken cancellationToken = default)
    {
        var vault = await vaultRepository.GetByIdAsync(vaultId, cancellationToken)
            ?? throw new KeyNotFoundException("Vault not found.");

        if (!vault.CanRead(actorId))
        {
            throw new UnauthorizedAccessException("No read access to vault.");
        }

        var docs = await documentRepository.GetByVaultIdAsync(vaultId, cancellationToken);
        return docs.Select(Map).ToArray();
    }

    private static void EnsureUploadRules(UploadDocumentRequest request)
    {
        const long maxSize = 100L * 1024L * 1024L;
        if (request.FileSize <= 0 || request.FileSize > maxSize)
        {
            throw new InvalidOperationException("File size exceeds allowed limit (100MB).");
        }

        var allowedMimeTypes = new[] { "application/pdf", "text/plain", "image/png", "image/jpeg", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
        if (!allowedMimeTypes.Contains(request.MimeType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Unsupported MIME type.");
        }
    }

    private static DocumentDto Map(Document document)
        => new(document.Id, document.VaultId, document.OriginalFileName, document.MimeType, document.FileSize, document.Sha256Hash, document.Classification, document.IsDeleted, document.CreatedAtUtc, document.Versions.Count);
}
