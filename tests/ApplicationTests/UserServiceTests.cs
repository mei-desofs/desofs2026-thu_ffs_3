using Moq;
using SafeVault.Application.DTOs.Users;
using SafeVault.Application.IServices;
using SafeVault.Application.Services;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Domain.IRepositories;

namespace SafeVault.ApplicationTests;

public class UserServiceTests
{
    [Fact]
    public async Task Create_ShouldPersistAndReturnDto()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(x => x.GetByEmailAsync("new@user.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed");

        var sut = new UserService(repo.Object, hasher.Object, Mock.Of<IAuditWriter>());

        var dto = await sut.CreateAsync(new CreateUserRequest("new@user.com", "ValidPassword!123", UserRole.Manager));

        Assert.Equal("new@user.com", dto.Email);
        repo.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldThrow_WhenUserMissing()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var sut = new UserService(repo.Object, Mock.Of<IPasswordHasher>(), Mock.Of<IAuditWriter>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            sut.UpdateAsync(Guid.NewGuid(), new UpdateUserRequest("u@x.com", UserRole.Viewer, true)));
    }
}
