namespace SafeVault.Domain.EntityModels;

public class DocumentVersion
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public int VersionNumber { get; private set; }
    public string StoredFileName { get; private set; }
    public string Sha256Hash { get; private set; }
    public Guid UploadedBy { get; private set; }
    public long FileSize { get; private set; }
    public DateTime UploadedAtUtc { get; private set; }

    private DocumentVersion()
    {
        StoredFileName = string.Empty;
        Sha256Hash = string.Empty;
    }

    public DocumentVersion(Guid documentId, int versionNumber, string storedFileName, string sha256Hash, Guid uploadedBy, long fileSize)
    {
        Id = Guid.NewGuid();
        DocumentId = documentId;
        VersionNumber = versionNumber;
        StoredFileName = storedFileName;
        Sha256Hash = sha256Hash;
        UploadedBy = uploadedBy;
        FileSize = fileSize;
        UploadedAtUtc = DateTime.UtcNow;
    }

    public static DocumentVersion Rehydrate(
        Guid id,
        Guid documentId,
        int versionNumber,
        string storedFileName,
        string sha256Hash,
        Guid uploadedBy,
        long fileSize,
        DateTime uploadedAtUtc)
    {
        return new DocumentVersion
        {
            Id = id,
            DocumentId = documentId,
            VersionNumber = versionNumber,
            StoredFileName = storedFileName,
            Sha256Hash = sha256Hash,
            UploadedBy = uploadedBy,
            FileSize = fileSize,
            UploadedAtUtc = uploadedAtUtc
        };
    }
}
