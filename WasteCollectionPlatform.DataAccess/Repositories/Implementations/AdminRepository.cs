using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

/// <summary>
/// Admin repository implementation
/// </summary>
public class AdminRepository : GenericRepository<Admin>, IAdminRepository
{
    public AdminRepository(WasteManagementContext context) : base(context)
    {
    }

    public async Task<Admin?> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.UserId == userId);
    }

    public async Task<Admin?> GetByIdWithDetailsAsync(int adminId)
    {
        return await _dbSet
            .Include(a => a.User)
            .Include(a => a.CreatorUser)
            .FirstOrDefaultAsync(a => a.Id == adminId);
    }

    public async Task<List<Admin>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(a => a.User)
            .Include(a => a.CreatorUser)
            .ToListAsync();
    }

    public async Task<bool> UserIsAdminAsync(int userId)
    {
        return await _dbSet
            .AnyAsync(a => a.UserId == userId && a.Status);
    }
}
