using System.Security.Cryptography;

namespace WasteCollectionPlatform.Common.Helpers;

/// <summary>
/// Helper class for generating secure refresh tokens
/// </summary>
public static class RefreshTokenHelper
{
    /// <summary>
    /// Generate a cryptographically secure random refresh token
    /// </summary>
    /// <returns>Base64 encoded random token</returns>
    public static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Calculate expiration date for refresh token (default 30 days)
    /// </summary>
    /// <param name="days">Number of days until expiration</param>
    /// <returns>Expiration DateTime in UTC</returns>
    public static DateTime CalculateExpirationDate(int days = 30)
    {
        return DateTime.UtcNow.AddDays(days);
    }
}
