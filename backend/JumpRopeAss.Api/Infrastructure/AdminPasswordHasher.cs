using System.Security.Cryptography;

namespace JumpRopeAss.Api.Infrastructure;

public static class AdminPasswordHasher
{
    public static string Hash(string password, int iterations = 100_000)
    {
        if (password is null) throw new ArgumentNullException(nameof(password));
        var salt = RandomNumberGenerator.GetBytes(16);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);
        return $"pbkdf2${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string storedHash, bool allowLegacyPlain = false)
    {
        if (password is null) return false;
        if (string.IsNullOrWhiteSpace(storedHash)) return false;

        if (!storedHash.StartsWith("pbkdf2$", StringComparison.Ordinal))
        {
            if (!allowLegacyPlain) return false;
            return string.Equals(password, storedHash, StringComparison.Ordinal);
        }

        var parts = storedHash.Split('$');
        if (parts.Length != 4) return false;
        if (!int.TryParse(parts[1], out var iterations)) return false;
        byte[] salt;
        byte[] hash;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            hash = Convert.FromBase64String(parts[3]);
        }
        catch
        {
            return false;
        }

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var computed = pbkdf2.GetBytes(hash.Length);
        return CryptographicOperations.FixedTimeEquals(computed, hash);
    }
}

