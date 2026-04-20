using SafeVault.Domain.Enums;

namespace SafeVault.Domain.EntityModels;

public class AuditLog
{
    public Guid Id { get; private set; }
    public AuditEventType EventType { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? TargetResourceId { get; private set; }
    public string TargetResourceType { get; private set; }
    public string IpAddress { get; private set; }
    public string UserAgent { get; private set; }
    public DateTime TimestampUtc { get; private set; }
    public bool Success { get; private set; }
    public string Details { get; private set; }

    private AuditLog()
    {
        TargetResourceType = string.Empty;
        IpAddress = string.Empty;
        UserAgent = string.Empty;
        Details = string.Empty;
    }

    public AuditLog(
        AuditEventType eventType,
        Guid? userId,
        Guid? targetResourceId,
        string targetResourceType,
        string ipAddress,
        string userAgent,
        bool success,
        string details)
    {
        Id = Guid.NewGuid();
        EventType = eventType;
        UserId = userId;
        TargetResourceId = targetResourceId;
        TargetResourceType = targetResourceType;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Success = success;
        Details = details;
        TimestampUtc = DateTime.UtcNow;
    }

    public static AuditLog Rehydrate(
        Guid id,
        AuditEventType eventType,
        Guid? userId,
        Guid? targetResourceId,
        string targetResourceType,
        string ipAddress,
        string userAgent,
        DateTime timestampUtc,
        bool success,
        string details)
    {
        return new AuditLog
        {
            Id = id,
            EventType = eventType,
            UserId = userId,
            TargetResourceId = targetResourceId,
            TargetResourceType = targetResourceType,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            TimestampUtc = timestampUtc,
            Success = success,
            Details = details
        };
    }
}
