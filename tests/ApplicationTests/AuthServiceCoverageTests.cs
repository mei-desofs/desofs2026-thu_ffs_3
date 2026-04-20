using Moq;
using SafeVault.Application.DTOs.Auth;
using SafeVault.Application.IServices;
using SafeVault.Application.Services;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Domain.IRepositories;

namespace SafeVault.ApplicationTests;

public class AuthServiceCoverageTests
{
    [Fact]
    public async Task Register_ShouldCreateUserIssueTokensAndAudit()
    {
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByEmailAsync("new@example.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(x => x.Hash("ValidPassword!123")).Returns("pwd-hash");

        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");
        tokenService.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns(("access-token", DateTime.UtcNow.AddMinutes(60)));

        var hashService = new Mock<IHashService>();
        hashService.Setup(x => x.ComputeSha256("refresh-token")).Returns("refresh-hash");

        var auditWriter = new Mock<IAuditWriter>();

        var sut = new AuthService(userRepository.Object, passwordHasher.Object, tokenService.Object, hashService.Object, auditWriter.Object);

        var result = await sut.RegisterAsync(new RegisterRequest("new@example.com", "ValidPassword!123", UserRole.Manager));

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        userRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        auditWriter.Verify(x => x.WriteAsync(AuditEventType.UserCreated, It.IsAny<Guid?>(), It.IsAny<Guid?>(), nameof(User), "n/a", "n/a", true, "User registered.", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_ShouldSucceedAndPersist_WhenCredentialsAreValid()
    {
        var user = new User("manager@example.com", "stored-hash", UserRole.Manager);

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(x => x.Verify("stored-hash", "ValidPassword!1")).Returns(true);

        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(x => x.GenerateRefreshToken()).Returns("refresh");
        tokenService.Setup(x => x.GenerateAccessToken(user)).Returns(("access", DateTime.UtcNow.AddMinutes(60)));

        var hashService = new Mock<IHashService>();
        hashService.Setup(x => x.ComputeSha256("refresh")).Returns("refresh-hash");

        var auditWriter = new Mock<IAuditWriter>();

        var sut = new AuthService(userRepository.Object, passwordHasher.Object, tokenService.Object, hashService.Object, auditWriter.Object);

        var result = await sut.LoginAsync(new LoginRequest(user.Email, "ValidPassword!1"), "127.0.0.1", "agent");

        Assert.Equal("access", result.AccessToken);
        userRepository.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        auditWriter.Verify(x => x.WriteAsync(AuditEventType.Login, user.Id, user.Id, nameof(User), "127.0.0.1", "agent", true, "Login succeeded.", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_ShouldThrow_WhenUserIsLocked()
    {
        var user = new User("locked@example.com", "hash", UserRole.Viewer);
        for (var i = 0; i < 5; i++)
        {
            user.RegisterFailedLoginAttempt();
        }

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var sut = new AuthService(userRepository.Object, Mock.Of<IPasswordHasher>(), Mock.Of<ITokenService>(), Mock.Of<IHashService>(), Mock.Of<IAuditWriter>());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.LoginAsync(new LoginRequest(user.Email, "AnyPassword!1"), "127.0.0.1", "agent"));
    }

    [Fact]
    public async Task RefreshToken_ShouldThrow_WhenNoMatchingTokenExists()
    {
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<User>());

        var hashService = new Mock<IHashService>();
        hashService.Setup(x => x.ComputeSha256("invalid-refresh")).Returns("invalid-hash");

        var sut = new AuthService(userRepository.Object, Mock.Of<IPasswordHasher>(), Mock.Of<ITokenService>(), hashService.Object, Mock.Of<IAuditWriter>());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => sut.RefreshTokenAsync(new RefreshTokenRequest("invalid-refresh")));
    }

    [Fact]
    public async Task Register_ShouldThrow_WhenPasswordIsWeak()
    {
        var sut = new AuthService(Mock.Of<IUserRepository>(), Mock.Of<IPasswordHasher>(), Mock.Of<ITokenService>(), Mock.Of<IHashService>(), Mock.Of<IAuditWriter>());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            sut.RegisterAsync(new RegisterRequest("new@example.com", "weak", UserRole.Viewer)));
    }
}
