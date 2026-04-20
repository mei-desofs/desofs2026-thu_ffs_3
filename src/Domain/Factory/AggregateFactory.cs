using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;

namespace SafeVault.Domain.Factory;

public static class AggregateFactory
{
    public static User CreateUser(string email, string passwordHash, UserRole role) => new(email, passwordHash, role);

    public static Vault CreateVault(string name, string description, Guid ownerId, string directoryPath, int retentionDays, bool autoDeleteOnExpiry)
        => new(name, description, ownerId, directoryPath, retentionDays, autoDeleteOnExpiry);

    public static Document CreateDocument(
        Guid vaultId,
        Guid uploadedBy,
        string originalFileName,
        string storedFileName,
        string filePath,
        string mimeType,
        long fileSize,
        string sha256Hash,
        DocumentClassification classification)
        => new(vaultId, uploadedBy, originalFileName, storedFileName, filePath, mimeType, fileSize, sha256Hash, classification);
}
