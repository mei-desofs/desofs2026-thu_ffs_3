using SafeVault.Domain.Enums;

namespace SafeVault.Application.DTOs.Auth;

public record RegisterRequest(string Email, string Password, UserRole Role);

public record LoginRequest(string Email, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken, DateTime RefreshTokenExpiresAtUtc);
