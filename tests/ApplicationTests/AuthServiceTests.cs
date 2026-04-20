using Moq;
using SafeVault.Application.DTOs.Auth;
using SafeVault.Application.IServices;
using SafeVault.Application.Services;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Domain.IRepositories;

namespace SafeVault.ApplicationTests;

public class AuthServiceTests
{
    [Fact]
    public async Task Login_ShouldThrow_WhenPasswordIsInvalid()
    {
        var user = new User("manager@example.com", "hashed", UserRole.Manager);

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(x => x.Verify(user.PasswordHash, "WrongPassword!1")).Returns(false);

        var tokenService = new Mock<ITokenService>();
        var hashService = new Mock<IHashService>();
        var auditWriter = new Mock<IAuditWriter>();

        var sut = new AuthService(userRepository.Object, passwordHasher.Object, tokenService.Object, hashService.Object, auditWriter.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.LoginAsync(new LoginRequest(user.Email, "WrongPassword!1"), "127.0.0.1", "test-agent"));
    }

    [Fact]
    public async Task Register_ShouldThrow_WhenEmailExists()
    {
        var existing = new User("admin@example.com", "hash", UserRole.Admin);
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByEmailAsync(existing.Email, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var sut = new AuthService(userRepository.Object, Mock.Of<IPasswordHasher>(), Mock.Of<ITokenService>(), Mock.Of<IHashService>(), Mock.Of<IAuditWriter>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.RegisterAsync(new RegisterRequest(existing.Email, "ValidPassword!123", UserRole.Manager)));
    }

    [Fact]
    public async Task RefreshToken_ShouldRotate_WhenValidTokenFound()
    {
        var user = new User("user@example.com", "hash", UserRole.Viewer);
        user.AddRefreshToken("incoming-hash", DateTime.UtcNow.AddMinutes(10));

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { user });

        var hashService = new Mock<IHashService>();
        hashService.Setup(x => x.ComputeSha256("incoming-token")).Returns("incoming-hash");
        hashService.Setup(x => x.ComputeSha256("new-refresh")).Returns("new-refresh-hash");

        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(x => x.GenerateRefreshToken()).Returns("new-refresh");
        tokenService.Setup(x => x.GenerateAccessToken(user)).Returns(("access-token", DateTime.UtcNow.AddMinutes(60)));

        var sut = new AuthService(userRepository.Object, Mock.Of<IPasswordHasher>(), tokenService.Object, hashService.Object, Mock.Of<IAuditWriter>());

        var result = await sut.RefreshTokenAsync(new RefreshTokenRequest("incoming-token"));

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("new-refresh", result.RefreshToken);
        userRepository.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }
}
