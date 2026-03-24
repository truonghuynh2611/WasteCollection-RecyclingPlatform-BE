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
                && rt.Isrevoked != true 
                && rt.Expiresat > now)
            .ToListAsync();
    }

    public async Task RevokeAllUserTokensAsync(int userId)
    {
        var tokens = await _dbSet
            .Where(rt => rt.UserId == userId && rt.Isrevoked != true)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.Isrevoked = true;
            token.Revokedat = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpiredTokensAsync()
    {
        var now = DateTime.UtcNow;
        var expiredTokens = await _dbSet
            .Where(rt => rt.Expiresat <= now || rt.Isrevoked == true)
            .ToListAsync();

        _dbSet.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
    }
}
