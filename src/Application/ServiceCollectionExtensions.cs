using Microsoft.Extensions.DependencyInjection;
using SafeVault.Application.IServices;
using SafeVault.Application.Services;

namespace SafeVault.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IVaultService, VaultService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}
