namespace SafeVault.Infrastructure.Options;

public class StorageOptions
{
    public const string SectionName = "Storage";

    public string BasePath { get; set; } = "storage";
}
