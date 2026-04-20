using SafeVault.Domain.Enums;

namespace SafeVault.Application.DTOs.Documents;

public record DocumentDto(
    Guid Id,
    Guid VaultId,
    string OriginalFileName,
    string MimeType,
    long FileSize,
    string Sha256Hash,
    DocumentClassification Classification,
    bool IsDeleted,
    DateTime CreatedAtUtc,
    int CurrentVersion);

public record UploadDocumentRequest(Guid VaultId, string OriginalFileName, string MimeType, long FileSize, DocumentClassification Classification, Stream Content);

public record DownloadDocumentResponse(string FileName, string MimeType, Stream Content);
