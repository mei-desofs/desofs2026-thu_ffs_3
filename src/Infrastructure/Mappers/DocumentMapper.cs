using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Infrastructure.DataModels;

namespace SafeVault.Infrastructure.Mappers;

public static class DocumentMapper
{
    public static Document ToDomain(DocumentDataModel model, IEnumerable<DocumentVersionDataModel> versionModels)
    {
        var versions = versionModels.Select(ToDomain).ToArray();

        return Document.Rehydrate(
            model.Id,
            model.VaultId,
            model.UploadedBy,
            model.OriginalFileName,
            model.StoredFileName,
            model.FilePath,
            model.MimeType,
            model.FileSize,
            model.Sha256Hash,
            Enum.Parse<DocumentClassification>(model.Classification),
            model.IsDeleted,
            model.DeletedAtUtc,
            model.CreatedAtUtc,
            versions);
    }

    public static DocumentDataModel ToDataModel(Document document)
        => new()
        {
            Id = document.Id,
            VaultId = document.VaultId,
            UploadedBy = document.UploadedBy,
            OriginalFileName = document.OriginalFileName,
            StoredFileName = document.StoredFileName,
            FilePath = document.FilePath,
            MimeType = document.MimeType,
            FileSize = document.FileSize,
            Sha256Hash = document.Sha256Hash,
            Classification = document.Classification.ToString(),
            IsDeleted = document.IsDeleted,
            DeletedAtUtc = document.DeletedAtUtc,
            CreatedAtUtc = document.CreatedAtUtc
        };

    public static DocumentVersionDataModel ToDataModel(DocumentVersion version)
        => new()
        {
            Id = version.Id,
            DocumentId = version.DocumentId,
            VersionNumber = version.VersionNumber,
            StoredFileName = version.StoredFileName,
            Sha256Hash = version.Sha256Hash,
            UploadedBy = version.UploadedBy,
            FileSize = version.FileSize,
            UploadedAtUtc = version.UploadedAtUtc
        };

    public static DocumentVersion ToDomain(DocumentVersionDataModel model)
        => DocumentVersion.Rehydrate(
            model.Id,
            model.DocumentId,
            model.VersionNumber,
            model.StoredFileName,
            model.Sha256Hash,
            model.UploadedBy,
            model.FileSize,
            model.UploadedAtUtc);
}
