using Microsoft.EntityFrameworkCore;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.IRepositories;
using SafeVault.Infrastructure.Mappers;

namespace SafeVault.Infrastructure.Repositories;

public class UserRepository(SafeVaultDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userModel = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (userModel is null)
        {
            return null;
        }

        var refreshModels = await dbContext.RefreshTokens.AsNoTracking().Where(x => x.UserId == id).ToArrayAsync(cancellationToken);
        return UserMapper.ToDomain(userModel, refreshModels);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var userModel = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (userModel is null)
        {
            return null;
        }

        var refreshModels = await dbContext.RefreshTokens.AsNoTracking().Where(x => x.UserId == userModel.Id).ToArrayAsync(cancellationToken);
        return UserMapper.ToDomain(userModel, refreshModels);
    }

    public async Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var userModels = await dbContext.Users.AsNoTracking().ToArrayAsync(cancellationToken);
        var tokenModels = await dbContext.RefreshTokens.AsNoTracking().ToArrayAsync(cancellationToken);

        return userModels
            .Select(u => UserMapper.ToDomain(u, tokenModels.Where(t => t.UserId == u.Id)))
            .ToArray();
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var userModel = UserMapper.ToDataModel(user);
        await dbContext.Users.AddAsync(userModel, cancellationToken);

        foreach (var token in user.RefreshTokens)
        {
            await dbContext.RefreshTokens.AddAsync(UserMapper.ToDataModel(token), cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == user.Id, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        existing.Email = user.Email;
        existing.PasswordHash = user.PasswordHash;
        existing.Role = user.Role.ToString();
        existing.IsActive = user.IsActive;
        existing.FailedLoginAttempts = user.FailedLoginAttempts;
        existing.LockoutUntilUtc = user.LockoutUntilUtc;
        existing.LastLoginAtUtc = user.LastLoginAtUtc;

        var existingTokens = await dbContext.RefreshTokens.Where(x => x.UserId == user.Id).ToArrayAsync(cancellationToken);
        dbContext.RefreshTokens.RemoveRange(existingTokens);
        foreach (var token in user.RefreshTokens)
        {
            await dbContext.RefreshTokens.AddAsync(UserMapper.ToDataModel(token), cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == user.Id, cancellationToken);
        if (existing is null)
        {
            return;
        }

        var existingTokens = await dbContext.RefreshTokens.Where(x => x.UserId == user.Id).ToArrayAsync(cancellationToken);
        dbContext.RefreshTokens.RemoveRange(existingTokens);
        dbContext.Users.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
