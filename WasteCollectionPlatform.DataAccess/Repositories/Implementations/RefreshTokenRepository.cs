using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

/// <summary>
/// Repository implementation for RefreshToken entity
/// </summary>
public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(WasteManagementContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _dbSet
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(int userId)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(rt => rt.UserId == userId 
                && rt.IsRevoked != true 
                && rt.ExpiresAt > now)
            .ToListAsync();
    }

    public async Task RevokeAllUserTokensAsync(int userId)
    {
        var tokens = await _dbSet
            .Where(rt => rt.UserId == userId && rt.IsRevoked != true)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpiredTokensAsync()
    {
        var now = DateTime.UtcNow;
        var expiredTokens = await _dbSet
            .Where(rt => rt.ExpiresAt <= now || rt.IsRevoked == true)
            .ToListAsync();

        _dbSet.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
    }
}
