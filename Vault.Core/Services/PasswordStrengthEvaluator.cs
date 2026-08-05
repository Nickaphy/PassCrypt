namespace Vault.Core.Services;

public static class PasswordStrengthEvaluator
{
    public static bool IsStrong(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 12)
        {
            return false;
        }

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSymbol = password.Any(ch => !char.IsLetterOrDigit(ch));
        return hasUpper && hasLower && hasDigit && hasSymbol;
    }
}
