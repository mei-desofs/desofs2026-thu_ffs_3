using Microsoft.Extensions.Options;
using SafeVault.Application.IServices;
using SafeVault.Infrastructure.Options;

namespace SafeVault.Infrastructure.Storage;

public class FileStorageService(IOptions<StorageOptions> options) : IFileStorageService
{
    private readonly string _basePath = Path.GetFullPath(options.Value.BasePath);

    public string CreateVaultDirectory(Guid vaultId, string vaultName)
    {
        var sanitized = new string(vaultName.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_' or ' ').ToArray()).Trim();
        var path = Path.Combine(_basePath, vaultId.ToString("N"), sanitized);

        Directory.CreateDirectory(path);
        return path;
    }

    public async Task<string> SaveFileAsync(Guid vaultId, string originalFileName, Stream content, CancellationToken cancellationToken = default)
    {
        var safeName = Path.GetFileName(originalFileName);
        var extension = Path.GetExtension(safeName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";

        var vaultDir = Path.Combine(_basePath, vaultId.ToString("N"));
        Directory.CreateDirectory(vaultDir);

        var fullPath = Path.GetFullPath(Path.Combine(vaultDir, storedFileName));
        if (!fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path traversal detected.");
        }

        await using var output = File.Create(fullPath);
        content.Position = 0;
        await content.CopyToAsync(output, cancellationToken);
        content.Position = 0;

        return fullPath;
    }

    public Task<Stream> ReadFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(filePath);
        if (!fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path traversal detected.");
        }

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(filePath);
        if (fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
