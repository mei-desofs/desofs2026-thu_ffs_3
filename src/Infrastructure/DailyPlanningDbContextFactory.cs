using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SafeVault.Infrastructure;

public class SafeVaultDbContextFactory : IDesignTimeDbContextFactory<SafeVaultDbContext>
{
    public SafeVaultDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SafeVaultDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=safevault;Username=postgres;Password=postgres");

        return new SafeVaultDbContext(optionsBuilder.Options);
    }
}
