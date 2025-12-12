using System.Security.Cryptography;

namespace DATABASPROJEKT___E_HANDEL_ADMIN.Services;

public static class HashingService
{
    public static string GenerateSalt(int size = 16)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(size);
        return Convert.ToBase64String(saltBytes);
    }

    public static string HashWithSalt(string value, string base64Salt, int iterations = 100_000, int hashLength = 32)
    {
        var saltBytes = Convert.FromBase64String(base64Salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password: value,
            salt: saltBytes,
            iterations: iterations,
            hashAlgorithm: HashAlgorithmName.SHA256);

        var hash = pbkdf2.GetBytes(hashLength);
        return Convert.ToBase64String(hash);
    }

    public static (string hash, string salt) HashPassword(string password)
    {
        var salt = GenerateSalt();
        var hash = HashWithSalt(password, salt);
        return (hash, salt);
    }

    public static string HashValue(string value, string salt)
    {
        return HashWithSalt(value, salt);
    }

    public static bool IsValidBase64Salt(string? salt)
    {
        if (string.IsNullOrWhiteSpace(salt))
            return false;

        try
        {
            Convert.FromBase64String(salt);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static bool Verify(string value, string base64Salt, string expectedBase64Hash)
    {
        var computedHash = HashWithSalt(value, base64Salt);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(expectedBase64Hash),
            Convert.FromBase64String(computedHash));
    }
}
