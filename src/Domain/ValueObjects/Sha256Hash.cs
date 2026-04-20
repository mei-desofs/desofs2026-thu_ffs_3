namespace SafeVault.Domain.ValueObjects;

public sealed class Sha256Hash
{
    public string Value { get; }

    public Sha256Hash(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 64)
        {
            throw new ArgumentException("SHA-256 hash must be a 64-char hex string.");
        }

        if (!value.All(c => Uri.IsHexDigit(c)))
        {
            throw new ArgumentException("SHA-256 hash contains non-hexadecimal characters.");
        }

        Value = value.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
