using SafeVault.Domain.Enums;

namespace SafeVault.Application.DTOs.Users;

public record UserDto(Guid Id, string Email, UserRole Role, bool IsActive, DateTime CreatedAtUtc, DateTime? LastLoginAtUtc);

public record CreateUserRequest(string Email, string Password, UserRole Role);

public record UpdateUserRequest(string Email, UserRole Role, bool IsActive);
