using System.Security.Cryptography;
using System.Text;

namespace GGHubApi.Services;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string providedPassword, string storedHash);
}

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public bool VerifyPassword(string providedPassword, string storedHash)
    {
        if (string.Equals(providedPassword, storedHash, StringComparison.Ordinal))
            return true;

        var hashed = HashPassword(providedPassword);
        return string.Equals(hashed, storedHash, StringComparison.Ordinal);
    }
}
