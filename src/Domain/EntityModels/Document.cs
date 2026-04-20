using SafeVault.Domain.Enums;
using SafeVault.Domain.ValueObjects;

namespace SafeVault.Domain.EntityModels;

public class Document
{
    private readonly List<DocumentVersion> _versions = [];

    public Guid Id { get; private set; }
    public Guid VaultId { get; private set; }
    public Guid UploadedBy { get; private set; }
    public string OriginalFileName { get; private set; }
    public string StoredFileName { get; private set; }
    public string FilePath { get; private set; }
    public string MimeType { get; private set; }
    public long FileSize { get; private set; }
    public string Sha256Hash { get; private set; }
    public DocumentClassification Classification { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<DocumentVersion> Versions => _versions.AsReadOnly();

    private Document()
    {
        OriginalFileName = string.Empty;
        StoredFileName = string.Empty;
        FilePath = string.Empty;
        MimeType = string.Empty;
        Sha256Hash = string.Empty;
    }

    public Document(
        Guid vaultId,
        Guid uploadedBy,
        string originalFileName,
        string storedFileName,
        string filePath,
        string mimeType,
        long fileSize,
        string sha256Hash,
        DocumentClassification classification)
    {
        Id = Guid.NewGuid();
        VaultId = vaultId;
        UploadedBy = uploadedBy;
        OriginalFileName = originalFileName;
        StoredFileName = storedFileName;
        FilePath = filePath;
        MimeType = mimeType;
        FileSize = fileSize;
        Sha256Hash = new Sha256Hash(sha256Hash).Value;
        Classification = classification;
        CreatedAtUtc = DateTime.UtcNow;

        _versions.Add(new DocumentVersion(Id, 1, storedFileName, Sha256Hash, uploadedBy, fileSize));
    }

    public static Document Rehydrate(
        Guid id,
        Guid vaultId,
        Guid uploadedBy,
        string originalFileName,
        string storedFileName,
        string filePath,
        string mimeType,
        long fileSize,
        string sha256Hash,
        DocumentClassification classification,
        bool isDeleted,
        DateTime? deletedAtUtc,
        DateTime createdAtUtc,
        IEnumerable<DocumentVersion>? versions = null)
    {
        var document = new Document
        {
            Id = id,
            VaultId = vaultId,
            UploadedBy = uploadedBy,
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            FilePath = filePath,
            MimeType = mimeType,
            FileSize = fileSize,
            Sha256Hash = sha256Hash,
            Classification = classification,
            IsDeleted = isDeleted,
            DeletedAtUtc = deletedAtUtc,
            CreatedAtUtc = createdAtUtc
        };

        if (versions is not null)
        {
            document._versions.Clear();
            document._versions.AddRange(versions);
        }

        return document;
    }

    public void AddVersion(string storedFileName, string filePath, string sha256Hash, Guid uploadedBy, long fileSize)
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Cannot add version to a deleted document.");
        }

        var versionNumber = _versions.Count + 1;
        StoredFileName = storedFileName;
        FilePath = filePath;
        Sha256Hash = new Sha256Hash(sha256Hash).Value;
        UploadedBy = uploadedBy;
        FileSize = fileSize;

        _versions.Add(new DocumentVersion(Id, versionNumber, storedFileName, Sha256Hash, uploadedBy, fileSize));
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
    }
}
