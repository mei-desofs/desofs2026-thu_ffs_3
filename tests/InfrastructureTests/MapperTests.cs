using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;
using SafeVault.Infrastructure.Mappers;

namespace SafeVault.InfrastructureTests;

public class MapperTests
{
    [Fact]
    public void DocumentMapper_ShouldMapToDataModel()
    {
        var doc = new Document(Guid.NewGuid(), Guid.NewGuid(), "a.pdf", "stored.pdf", "c:/tmp/stored.pdf", "application/pdf", 10, new string('a', 64), DocumentClassification.Confidential);

        var model = DocumentMapper.ToDataModel(doc);

        Assert.Equal(doc.Id, model.Id);
        Assert.Equal("Confidential", model.Classification);
    }

    [Fact]
    public void UserMapper_ShouldMapRoundTrip()
    {
        var user = new User("user@example.com", "hash", UserRole.Viewer);
        user.AddRefreshToken("tokenhash", DateTime.UtcNow.AddHours(1));

        var userModel = UserMapper.ToDataModel(user);
        var tokenModels = user.RefreshTokens.Select(UserMapper.ToDataModel).ToArray();

        var mapped = UserMapper.ToDomain(userModel, tokenModels);

        Assert.Equal(user.Email, mapped.Email);
        Assert.Single(mapped.RefreshTokens);
    }
}
