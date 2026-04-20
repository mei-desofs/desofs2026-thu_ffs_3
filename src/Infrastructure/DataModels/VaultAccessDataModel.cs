namespace SafeVault.Infrastructure.DataModels;

public class VaultAccessDataModel
{
    public Guid Id { get; set; }
    public Guid VaultId { get; set; }
    public Guid UserId { get; set; }
    public Guid GrantedBy { get; set; }
    public DateTime GrantedAtUtc { get; set; }
    public string AccessLevel { get; set; } = string.Empty;
}
