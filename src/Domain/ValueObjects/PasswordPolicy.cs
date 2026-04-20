namespace SafeVault.Domain.ValueObjects;

public static class PasswordPolicy
{
    public static void Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.");
        }

        if (password.Length < 12)
        {
            throw new ArgumentException("Password must be at least 12 characters long.");
        }

        if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit) || !password.Any(IsSpecial))
        {
            throw new ArgumentException("Password must include upper, lower, number and special character.");
        }
    }

    private static bool IsSpecial(char c) => !char.IsLetterOrDigit(c);
}
