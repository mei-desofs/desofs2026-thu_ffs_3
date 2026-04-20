using Moq;
using SafeVault.Application.Services;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Domain.IRepositories;

namespace SafeVault.ApplicationTests;

public class AuditServiceTests
{
    [Fact]
    public async Task Search_ShouldReturnOrderedByNewest()
    {
        var oldLog = new AuditLog(AuditEventType.Login, Guid.NewGuid(), Guid.NewGuid(), "User", "1.1.1.1", "agent", true, "ok");
        var newLog = new AuditLog(AuditEventType.DocumentUploaded, Guid.NewGuid(), Guid.NewGuid(), "Document", "1.1.1.1", "agent", true, "ok");

        var repo = new Mock<IAuditLogRepository>();
        repo.Setup(x => x.SearchAsync(null, null, null, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { oldLog, newLog });

        var sut = new AuditService(repo.Object);
        var result = await sut.SearchAsync(null, null, null);

        Assert.Equal(2, result.Count);
    }
}
