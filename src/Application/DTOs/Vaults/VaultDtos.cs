using SafeVault.Domain.Enums;

namespace SafeVault.Application.DTOs.Vaults;

public record VaultDto(Guid Id, string Name, string Description, Guid OwnerId, int RetentionDays, bool AutoDeleteOnExpiry, bool IsArchived, DateTime CreatedAtUtc);

public record CreateVaultRequest(string Name, string Description, int RetentionDays, bool AutoDeleteOnExpiry);

public record UpdateVaultRequest(string Name, string Description, int RetentionDays, bool AutoDeleteOnExpiry);

public record GrantVaultAccessRequest(Guid UserId, AccessLevel AccessLevel);
