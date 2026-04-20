using SafeVault.Domain.EntityModels;
using SafeVault.Domain.Enums;

namespace SafeVault.DomainTests;

public class DocumentDomainTests
{
    [Fact]
    public void Constructor_ShouldCreateFirstVersion()
    {
        var doc = new Document(Guid.NewGuid(), Guid.NewGuid(), "a.pdf", "stored.pdf", "c:/tmp/stored.pdf", "application/pdf", 1024, new string('a', 64), DocumentClassification.Confidential);

        Assert.Single(doc.Versions);
        Assert.Equal(1, doc.Versions.First().VersionNumber);
    }

    [Fact]
    public void AddVersion_ShouldIncreaseVersionCount()
    {
        var doc = new Document(Guid.NewGuid(), Guid.NewGuid(), "a.pdf", "stored.pdf", "c:/tmp/stored.pdf", "application/pdf", 1024, new string('a', 64), DocumentClassification.Confidential);

        doc.AddVersion("stored-v2.pdf", "c:/tmp/stored-v2.pdf", new string('b', 64), Guid.NewGuid(), 2048);

        Assert.Equal(2, doc.Versions.Count);
        Assert.Equal("stored-v2.pdf", doc.StoredFileName);
    }

    [Fact]
    public void AddVersion_ShouldThrowWhenDeleted()
    {
        var doc = new Document(Guid.NewGuid(), Guid.NewGuid(), "a.pdf", "stored.pdf", "c:/tmp/stored.pdf", "application/pdf", 1024, new string('a', 64), DocumentClassification.Confidential);
        doc.SoftDelete();

        Assert.Throws<InvalidOperationException>(() => doc.AddVersion("v2.pdf", "c:/tmp/v2.pdf", new string('b', 64), Guid.NewGuid(), 20));
    }
}
