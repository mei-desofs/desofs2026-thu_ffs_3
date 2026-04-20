using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SafeVault.Application.IServices;
using SafeVault.Domain.IRepositories;
using SafeVault.Infrastructure.Options;
using SafeVault.Infrastructure.Repositories;
using SafeVault.Infrastructure.Security;
using SafeVault.Infrastructure.Storage;

namespace SafeVault.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var storageOptions = configuration.GetSection(StorageOptions.SectionName).Get<StorageOptions>() ?? new StorageOptions();

        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(jwtOptions));
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(storageOptions));

        services.AddDbContext<SafeVaultDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVaultRepository, VaultRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddScoped<IHashService, HashService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IAuditWriter, AuditWriterService>();

        return services;
    }
}
