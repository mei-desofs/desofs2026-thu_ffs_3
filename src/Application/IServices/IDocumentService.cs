using SafeVault.Application.DTOs.Documents;

namespace SafeVault.Application.IServices;

public interface IDocumentService
{
    Task<DocumentDto> UploadAsync(Guid actorId, UploadDocumentRequest request, CancellationToken cancellationToken = default);
    Task<DocumentDto> UploadNewVersionAsync(Guid actorId, Guid documentId, UploadDocumentRequest request, CancellationToken cancellationToken = default);
    Task<DownloadDocumentResponse> DownloadAsync(Guid actorId, Guid documentId, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid actorId, Guid documentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DocumentDto>> ListByVaultAsync(Guid actorId, Guid vaultId, CancellationToken cancellationToken = default);
}
