using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

/// <summary>
/// Repository interface for RefreshToken entity
/// </summary>
public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    /// <summary>
    /// Get refresh token by token value
    /// </summary>
    Task<RefreshToken?> GetByTokenAsync(string token);

    /// <summary>
    /// Get all active (non-revoked, non-expired) tokens for a user
    /// </summary>
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(int userId);

    /// <summary>
    /// Revoke all tokens for a user (for logout all devices)
    /// </summary>
    Task RevokeAllUserTokensAsync(int userId);

    /// <summary>
    /// Delete expired tokens (cleanup)
    /// </summary>
    Task DeleteExpiredTokensAsync();
}
