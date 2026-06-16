using Microsoft.EntityFrameworkCore;
using SafeVault.Infrastructure.DataModels;

namespace SafeVault.Infrastructure;

public class SafeVaultDbContext(DbContextOptions<SafeVaultDbContext> options) : DbContext(options)
{
    public DbSet<UserDataModel> Users => Set<UserDataModel>();
    public DbSet<RefreshTokenDataModel> RefreshTokens => Set<RefreshTokenDataModel>();
    public DbSet<VaultDataModel> Vaults => Set<VaultDataModel>();
    public DbSet<VaultAccessDataModel> VaultAccesses => Set<VaultAccessDataModel>();
    public DbSet<DocumentDataModel> Documents => Set<DocumentDataModel>();
    public DbSet<DocumentVersionDataModel> DocumentVersions => Set<DocumentVersionDataModel>();
    public DbSet<AuditLogDataModel> AuditLogs => Set<AuditLogDataModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserDataModel>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<RefreshTokenDataModel>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            entity.HasOne<UserDataModel>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VaultDataModel>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(400).IsRequired();
            entity.Property(x => x.DirectoryPath).HasMaxLength(600).IsRequired();
            entity.HasOne<UserDataModel>().WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VaultAccessDataModel>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.VaultId, x.UserId }).IsUnique();
            entity.Property(x => x.AccessLevel).HasMaxLength(20).IsRequired();
            entity.HasOne<VaultDataModel>().WithMany().HasForeignKey(x => x.VaultId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentDataModel>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.FilePath).HasMaxLength(700).IsRequired();
            entity.Property(x => x.MimeType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Sha256Hash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Classification).HasMaxLength(20).IsRequired();
            entity.HasOne<VaultDataModel>().WithMany().HasForeignKey(x => x.VaultId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentVersionDataModel>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Sha256Hash).HasMaxLength(64).IsRequired();
            entity.HasOne<DocumentDataModel>().WithMany().HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLogDataModel>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(60).IsRequired();
            entity.Property(x => x.TargetResourceType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.IpAddress).HasMaxLength(45).IsRequired();
            entity.Property(x => x.UserAgent).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Details).HasMaxLength(1000).IsRequired();
            entity.HasIndex(x => x.TimestampUtc);
        });

        base.OnModelCreating(modelBuilder);
    }
}
