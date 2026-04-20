using SafeVault.Application.DTOs.Auth;
using SafeVault.Application.IServices;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Domain.IRepositories;
using SafeVault.Domain.ValueObjects;

namespace SafeVault.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IHashService hashService,
    IAuditWriter auditWriter) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        PasswordPolicy.Validate(request.Password);

        var existing = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = new User(request.Email, passwordHash, request.Role);

        var refreshToken = tokenService.GenerateRefreshToken();
        var refreshTokenHash = hashService.ComputeSha256(refreshToken);
        var refreshExpiresAt = DateTime.UtcNow.AddDays(7);
        user.AddRefreshToken(refreshTokenHash, refreshExpiresAt);

        await userRepository.AddAsync(user, cancellationToken);

        var access = tokenService.GenerateAccessToken(user);

        await auditWriter.WriteAsync(
            AuditEventType.UserCreated,
            user.Id,
            user.Id,
            nameof(User),
            "n/a",
            "n/a",
            true,
            "User registered.",
            cancellationToken);

        return new AuthResponse(access.Token, access.ExpiresAtUtc, refreshToken, refreshExpiresAt);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!user.IsActive || user.IsLocked())
        {
            throw new UnauthorizedAccessException("User is inactive or locked.");
        }

        if (!passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            user.RegisterFailedLoginAttempt();
            await userRepository.UpdateAsync(user, cancellationToken);

            await auditWriter.WriteAsync(
                AuditEventType.LoginFailed,
                user.Id,
                user.Id,
                nameof(User),
                ipAddress,
                userAgent,
                false,
                "Invalid credentials.",
                cancellationToken);

            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        user.RegisterSuccessfulLogin();

        var refreshToken = tokenService.GenerateRefreshToken();
        var refreshTokenHash = hashService.ComputeSha256(refreshToken);
        var refreshExpiresAt = DateTime.UtcNow.AddDays(7);
        user.AddRefreshToken(refreshTokenHash, refreshExpiresAt);

        await userRepository.UpdateAsync(user, cancellationToken);

        var access = tokenService.GenerateAccessToken(user);

        await auditWriter.WriteAsync(
            AuditEventType.Login,
            user.Id,
            user.Id,
            nameof(User),
            ipAddress,
            userAgent,
            true,
            "Login succeeded.",
            cancellationToken);

        return new AuthResponse(access.Token, access.ExpiresAtUtc, refreshToken, refreshExpiresAt);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var incomingHash = hashService.ComputeSha256(request.RefreshToken);
        var users = await userRepository.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(x => x.RefreshTokens.Any(t => !t.IsRevoked && t.TokenHash == incomingHash && t.ExpiresAtUtc > DateTime.UtcNow));

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        user.RevokeRefreshToken(incomingHash);

        var rotatedToken = tokenService.GenerateRefreshToken();
        var rotatedHash = hashService.ComputeSha256(rotatedToken);
        var refreshExpiresAt = DateTime.UtcNow.AddDays(7);
        user.AddRefreshToken(rotatedHash, refreshExpiresAt);

        await userRepository.UpdateAsync(user, cancellationToken);
        var access = tokenService.GenerateAccessToken(user);

        await auditWriter.WriteAsync(
            AuditEventType.RefreshTokenIssued,
            user.Id,
            user.Id,
            nameof(User),
            "n/a",
            "n/a",
            true,
            "Refresh token rotation performed.",
            cancellationToken);

        return new AuthResponse(access.Token, access.ExpiresAtUtc, rotatedToken, refreshExpiresAt);
    }
}
