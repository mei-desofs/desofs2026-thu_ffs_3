using Microsoft.EntityFrameworkCore;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Infrastructure;
using SafeVault.Infrastructure.Repositories;

namespace SafeVault.InfrastructureTests;

public class UserRepositoryTests
{
    [Fact]
    public async Task AddAndGetByEmail_ShouldPersistUser()
    {
        var options = new DbContextOptionsBuilder<SafeVaultDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new SafeVaultDbContext(options);
        var repo = new UserRepository(db);

        var user = new User("repo@test.com", "hash", UserRole.Manager);
        await repo.AddAsync(user);

        var loaded = await repo.GetByEmailAsync("repo@test.com");

        Assert.NotNull(loaded);
        Assert.Equal("repo@test.com", loaded!.Email);
    }
}
