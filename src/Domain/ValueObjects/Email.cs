using System.Text.RegularExpressions;

namespace SafeVault.Domain.ValueObjects;

public sealed class Email
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email is required.");
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!EmailRegex.IsMatch(normalized))
        {
            throw new ArgumentException("Invalid email format.");
        }

        Value = normalized;
    }

    public override string ToString() => Value;
}
