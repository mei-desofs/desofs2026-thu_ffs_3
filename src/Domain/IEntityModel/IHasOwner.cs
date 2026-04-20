namespace SafeVault.Domain.IEntityModel;

public interface IHasOwner
{
    Guid OwnerId { get; }
}
