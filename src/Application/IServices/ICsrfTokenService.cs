namespace SafeVault.Application.IServices;

public interface ICsrfTokenService
{
    string IssueToken(Guid userId);
    bool TryValidate(string token, Guid userId);
}
