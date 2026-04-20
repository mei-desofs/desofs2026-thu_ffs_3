using Microsoft.EntityFrameworkCore;
using SafeVault.Domain.EntityModels;
using SafeVault.Domain.IRepositories;
using SafeVault.Infrastructure.Mappers;

namespace SafeVault.Infrastructure.Repositories;

public class DocumentRepository(SafeVaultDbContext dbContext) : IDocumentRepository
{
    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var docModel = await dbContext.Documents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (docModel is null)
        {
            return null;
        }

        var versionModels = await dbContext.DocumentVersions.AsNoTracking().Where(x => x.DocumentId == id).ToArrayAsync(cancellationToken);
        return DocumentMapper.ToDomain(docModel, versionModels);
    }

    public async Task<IReadOnlyCollection<Document>> GetByVaultIdAsync(Guid vaultId, CancellationToken cancellationToken = default)
    {
        var docModels = await dbContext.Documents.AsNoTracking().Where(x => x.VaultId == vaultId).ToArrayAsync(cancellationToken);
        var versionModels = await dbContext.DocumentVersions.AsNoTracking().Where(x => docModels.Select(d => d.Id).Contains(x.DocumentId)).ToArrayAsync(cancellationToken);

        return docModels.Select(d => DocumentMapper.ToDomain(d, versionModels.Where(v => v.DocumentId == d.Id))).ToArray();
    }

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await dbContext.Documents.AddAsync(DocumentMapper.ToDataModel(document), cancellationToken);
        foreach (var version in document.Versions)
        {
            await dbContext.DocumentVersions.AddAsync(DocumentMapper.ToDataModel(version), cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Documents.FirstOrDefaultAsync(x => x.Id == document.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Document not found.");

        existing.UploadedBy = document.UploadedBy;
        existing.OriginalFileName = document.OriginalFileName;
        existing.StoredFileName = document.StoredFileName;
        existing.FilePath = document.FilePath;
        existing.MimeType = document.MimeType;
        existing.FileSize = document.FileSize;
        existing.Sha256Hash = document.Sha256Hash;
        existing.Classification = document.Classification.ToString();
        existing.IsDeleted = document.IsDeleted;
        existing.DeletedAtUtc = document.DeletedAtUtc;

        var existingVersions = await dbContext.DocumentVersions.Where(x => x.DocumentId == document.Id).ToArrayAsync(cancellationToken);
        dbContext.DocumentVersions.RemoveRange(existingVersions);
        foreach (var version in document.Versions)
        {
            await dbContext.DocumentVersions.AddAsync(DocumentMapper.ToDataModel(version), cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
