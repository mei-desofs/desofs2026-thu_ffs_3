namespace SafeVault.Application.IServices;

public interface IHashService
{
    string ComputeSha256(Stream content);
    string ComputeSha256(string plainText);
}
