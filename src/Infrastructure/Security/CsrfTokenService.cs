using Microsoft.AspNetCore.DataProtection;
using SafeVault.Application.IServices;

namespace SafeVault.Infrastructure.Security;

public class CsrfTokenService(IDataProtectionProvider provider) : ICsrfTokenService
{
    private static readonly TimeSpan TokenTtl = TimeSpan.FromMinutes(30);
    private readonly IDataProtector _protector = provider.CreateProtector("SafeVault.CsrfToken.v1");

    public string IssueToken(Guid userId)
    {
        var expiresAt = DateTimeOffset.UtcNow.Add(TokenTtl);
        var payload = $"{userId:N}|{expiresAt:O}";
        return _protector.Protect(payload);
    }

    public bool TryValidate(string token, Guid userId)
    {
        try
        {
            var payload = _protector.Unprotect(token);
            var parts = payload.Split('|');
            if (parts.Length != 2)
            {
                return false;
            }

            if (!Guid.TryParseExact(parts[0], "N", out var parsedUserId))
            {
                return false;
            }

            if (parsedUserId != userId)
            {
                return false;
            }

            if (!DateTimeOffset.TryParse(parts[1], out var expiresAt))
            {
                return false;
            }

            return expiresAt >= DateTimeOffset.UtcNow;
        }
        catch
        {
            return false;
        }
    }
}
