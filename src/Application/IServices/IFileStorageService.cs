namespace SafeVault.Application.IServices;

public interface IFileStorageService
{
    string CreateVaultDirectory(Guid vaultId, string vaultName);
    Task<string> SaveFileAsync(Guid vaultId, string originalFileName, Stream content, CancellationToken cancellationToken = default);
    Task<Stream> ReadFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
}
