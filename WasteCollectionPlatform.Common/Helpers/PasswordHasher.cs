namespace WasteCollectionPlatform.Common.Helpers;

/// <summary>
/// Password hashing utility using BCrypt
/// </summary>
public static class PasswordHasher
{
    /// <summary>
    /// Hash a password using BCrypt with work factor of 12
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }
    
    /// <summary>
    /// Verify a password against a hash
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hash">Hashed password to verify against</param>
    /// <returns>True if password matches hash, false otherwise</returns>
    public static bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
