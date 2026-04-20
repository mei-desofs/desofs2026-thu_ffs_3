using SafeVault.Domain.Enums;
using SafeVault.Domain.ValueObjects;

namespace SafeVault.Domain.EntityModels;

public class Vault
{
    private readonly List<VaultAccess> _accesses = [];

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Guid OwnerId { get; private set; }
    public string DirectoryPath { get; private set; }
    public int RetentionDays { get; private set; }
    public bool AutoDeleteOnExpiry { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<VaultAccess> Accesses => _accesses.AsReadOnly();

    private Vault()
    {
        Name = string.Empty;
        Description = string.Empty;
        DirectoryPath = string.Empty;
    }

    public Vault(string name, string description, Guid ownerId, string directoryPath, int retentionDays, bool autoDeleteOnExpiry)
    {
        Id = Guid.NewGuid();
        Name = new VaultName(name).Value;
        Description = description;
        OwnerId = ownerId;
        DirectoryPath = directoryPath;
        RetentionDays = retentionDays;
        AutoDeleteOnExpiry = autoDeleteOnExpiry;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Vault Rehydrate(
        Guid id,
        string name,
        string description,
        Guid ownerId,
        string directoryPath,
        int retentionDays,
        bool autoDeleteOnExpiry,
        bool isArchived,
        DateTime createdAtUtc,
        IEnumerable<VaultAccess>? accesses = null)
    {
        var vault = new Vault
        {
            Id = id,
            Name = name,
            Description = description,
            OwnerId = ownerId,
            DirectoryPath = directoryPath,
            RetentionDays = retentionDays,
            AutoDeleteOnExpiry = autoDeleteOnExpiry,
            IsArchived = isArchived,
            CreatedAtUtc = createdAtUtc
        };

        if (accesses is not null)
        {
            vault._accesses.Clear();
            vault._accesses.AddRange(accesses);
        }

        return vault;
    }

    public void UpdateDetails(string name, string description, int retentionDays, bool autoDeleteOnExpiry)
    {
        Name = new VaultName(name).Value;
        Description = description;
        RetentionDays = retentionDays;
        AutoDeleteOnExpiry = autoDeleteOnExpiry;
    }

    public void SetDirectoryPath(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Directory path is required.");
        }

        DirectoryPath = directoryPath;
    }

    public void Archive() => IsArchived = true;

    public void GrantAccess(Guid userId, Guid grantedBy, AccessLevel accessLevel)
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("Cannot grant access to an archived vault.");
        }

        var existing = _accesses.FirstOrDefault(x => x.UserId == userId);
        if (existing is not null)
        {
            existing.UpdateLevel(accessLevel);
            return;
        }

        _accesses.Add(new VaultAccess(Id, userId, grantedBy, accessLevel));
    }

    public void RevokeAccess(Guid userId)
    {
        var existing = _accesses.FirstOrDefault(x => x.UserId == userId);
        if (existing is not null)
        {
            _accesses.Remove(existing);
        }
    }

    public bool CanRead(Guid userId) => userId == OwnerId || _accesses.Any(x => x.UserId == userId);
    public bool CanWrite(Guid userId) => userId == OwnerId || _accesses.Any(x => x.UserId == userId && x.AccessLevel == AccessLevel.ReadWrite);
}
