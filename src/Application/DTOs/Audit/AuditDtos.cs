using SafeVault.Domain.Enums;

namespace SafeVault.Application.DTOs.Audit;

public record AuditLogDto(
    Guid Id,
    AuditEventType EventType,
    Guid? UserId,
    Guid? TargetResourceId,
    string TargetResourceType,
    string IpAddress,
    string UserAgent,
    DateTime TimestampUtc,
    bool Success,
    string Details);
