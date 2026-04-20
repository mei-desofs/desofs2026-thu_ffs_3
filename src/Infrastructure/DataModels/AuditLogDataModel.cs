namespace SafeVault.Infrastructure.DataModels;

public class AuditLogDataModel
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public Guid? TargetResourceId { get; set; }
    public string TargetResourceType { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
    public bool Success { get; set; }
    public string Details { get; set; } = string.Empty;
}
