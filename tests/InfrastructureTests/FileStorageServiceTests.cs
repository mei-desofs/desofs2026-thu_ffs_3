using Microsoft.Extensions.Options;
using SafeVault.Infrastructure.Options;
using SafeVault.Infrastructure.Storage;

namespace SafeVault.InfrastructureTests;

public class FileStorageServiceTests : IDisposable
{
    private readonly string _basePath;

    public FileStorageServiceTests()
    {
        _basePath = Path.Combine(Path.GetTempPath(), "safevault-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_basePath);
    }

    [Fact]
    public async Task SaveReadDelete_ShouldWork()
    {
        var sut = new FileStorageService(Options.Create(new StorageOptions { BasePath = _basePath }));
        var vaultId = Guid.NewGuid();

        await using var content = new MemoryStream([1, 2, 3, 4]);
        var path = await sut.SaveFileAsync(vaultId, "doc.pdf", content);

        await using (var read = await sut.ReadFileAsync(path))
        {
            Assert.NotNull(read);
        }

        await sut.DeleteFileAsync(path);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void CreateVaultDirectory_ShouldCreateFolder()
    {
        var sut = new FileStorageService(Options.Create(new StorageOptions { BasePath = _basePath }));
        var folder = sut.CreateVaultDirectory(Guid.NewGuid(), "Finance Vault");

        Assert.True(Directory.Exists(folder));
    }

    public void Dispose()
    {
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, true);
        }
    }
}
