using System.ComponentModel.DataAnnotations;
using SafeVault.Domain.Enums;

namespace SafeVault.Application.DTOs.Auth;

public record RegisterRequest(
    [Required] string Email,
    [Required] string Password,
    [Required] UserRole Role);

public record LoginRequest(
    [Required] string Email,
    [Required] string Password);

public record RefreshTokenRequest(
    [Required] string RefreshToken);

public record AuthResponse(string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken, DateTime RefreshTokenExpiresAtUtc);

public record CsrfTokenResponse(string Token, DateTime ExpiresAtUtc);
