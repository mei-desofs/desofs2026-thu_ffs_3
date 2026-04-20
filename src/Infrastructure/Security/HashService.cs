using System.Security.Cryptography;
using System.Text;
using SafeVault.Application.IServices;

namespace SafeVault.Infrastructure.Security;

public class HashService : IHashService
{
    public string ComputeSha256(Stream content)
    {
        content.Position = 0;
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(content);
        content.Position = 0;
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public string ComputeSha256(string plainText)
    {
        var bytes = Encoding.UTF8.GetBytes(plainText);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
