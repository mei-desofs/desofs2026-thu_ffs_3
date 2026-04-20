namespace SafeVault.Domain.Enums;

public enum AuditEventType
{
    Login = 1,
    LoginFailed = 2,
    RefreshTokenIssued = 3,
    UserCreated = 4,
    UserUpdated = 5,
    UserDisabled = 6,
    UserDeleted = 7,
    VaultCreated = 8,
    VaultUpdated = 9,
    VaultArchived = 10,
    VaultDeleted = 11,
    VaultAccessGranted = 12,
    VaultAccessRevoked = 13,
    DocumentUploaded = 14,
    DocumentDownloaded = 15,
    DocumentDeleted = 16,
    IntegrityFailure = 17,
    PermissionDenied = 18
}
