using SafeVault.Domain.EntityModels;

namespace SafeVault.Application.IServices;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
