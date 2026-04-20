using SafeVault.Domain.EntityModels;

namespace SafeVault.Domain.Visitors;

public interface IDomainVisitor
{
    void Visit(User user);
    void Visit(Vault vault);
    void Visit(Document document);
}
