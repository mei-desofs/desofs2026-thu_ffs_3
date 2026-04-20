namespace SafeVault.Infrastructure.DataModels;

public class DocumentDataModel
{
    public Guid Id { get; set; }
    public Guid VaultId { get; set; }
    public Guid UploadedBy { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Sha256Hash { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
