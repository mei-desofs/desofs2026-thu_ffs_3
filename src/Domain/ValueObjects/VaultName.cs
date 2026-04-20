using System.Text.RegularExpressions;

namespace SafeVault.Domain.ValueObjects;

public sealed class VaultName
{
    private static readonly Regex AllowedRegex = new(@"^[a-zA-Z0-9\-_\s]{3,100}$", RegexOptions.Compiled);

    public string Value { get; }

    public VaultName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Vault name is required.");
        }

        var trimmed = value.Trim();
        if (!AllowedRegex.IsMatch(trimmed))
        {
            throw new ArgumentException("Vault name contains invalid characters or invalid length.");
        }

        Value = trimmed;
    }

    public override string ToString() => Value;
}
