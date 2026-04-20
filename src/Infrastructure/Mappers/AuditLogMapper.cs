using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Infrastructure.DataModels;

namespace SafeVault.Infrastructure.Mappers;

public static class AuditLogMapper
{
    public static AuditLogDataModel ToDataModel(AuditLog log)
        => new()
        {
            Id = log.Id,
            EventType = log.EventType.ToString(),
            UserId = log.UserId,
            TargetResourceId = log.TargetResourceId,
            TargetResourceType = log.TargetResourceType,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            TimestampUtc = log.TimestampUtc,
            Success = log.Success,
            Details = log.Details
        };

    public static AuditLog ToDomain(AuditLogDataModel model)
        => AuditLog.Rehydrate(
            model.Id,
            Enum.Parse<AuditEventType>(model.EventType),
            model.UserId,
            model.TargetResourceId,
            model.TargetResourceType,
            model.IpAddress,
            model.UserAgent,
            model.TimestampUtc,
            model.Success,
            model.Details);
}
