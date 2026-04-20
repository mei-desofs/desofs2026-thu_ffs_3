using SafeVault.Application.DTOs.Users;
using SafeVault.Application.IServices;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Domain.IRepositories;
using SafeVault.Domain.ValueObjects;

namespace SafeVault.Application.Services;

public class UserService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IAuditWriter auditWriter) : IUserService
{
    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        PasswordPolicy.Validate(request.Password);

        if (await userRepository.GetByEmailAsync(request.Email, cancellationToken) is not null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var user = new User(request.Email, passwordHasher.Hash(request.Password), request.Role);
        await userRepository.AddAsync(user, cancellationToken);

        await auditWriter.WriteAsync(AuditEventType.UserCreated, user.Id, user.Id, nameof(User), "n/a", "n/a", true, "User created by admin.", cancellationToken);
        return Map(user);
    }

    public async Task<IReadOnlyCollection<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        return users.Select(Map).ToArray();
    }

    public async Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        return user is null ? null : Map(user);
    }

    public async Task<UserDto> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        user.Update(request.Email, request.Role);
        if (!request.IsActive)
        {
            user.Deactivate();
        }

        await userRepository.UpdateAsync(user, cancellationToken);
        await auditWriter.WriteAsync(AuditEventType.UserUpdated, user.Id, user.Id, nameof(User), "n/a", "n/a", true, "User updated by admin.", cancellationToken);

        return Map(user);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        await userRepository.DeleteAsync(user, cancellationToken);
        await auditWriter.WriteAsync(AuditEventType.UserDeleted, userId, userId, nameof(User), "n/a", "n/a", true, "User deleted by admin.", cancellationToken);
    }

    private static UserDto Map(User user)
        => new(user.Id, user.Email, user.Role, user.IsActive, user.CreatedAtUtc, user.LastLoginAtUtc);
}
