namespace SafeVault.Infrastructure.DataModels;

public class VaultDataModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string DirectoryPath { get; set; } = string.Empty;
    public int RetentionDays { get; set; }
    public bool AutoDeleteOnExpiry { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
