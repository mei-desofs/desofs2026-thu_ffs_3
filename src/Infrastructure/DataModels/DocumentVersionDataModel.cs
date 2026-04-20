namespace SafeVault.Infrastructure.DataModels;

public class DocumentVersionDataModel
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public string StoredFileName { get; set; } = string.Empty;
    public string Sha256Hash { get; set; } = string.Empty;
    public Guid UploadedBy { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadedAtUtc { get; set; }
}
