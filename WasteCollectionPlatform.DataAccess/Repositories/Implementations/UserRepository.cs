using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

/// <summary>
/// User repository implementation
/// </summary>
public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(WasteManagementContext context) : base(context)
    {
    }
    
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }
    
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }
    
    public async Task<User?> GetByIdWithDetailsAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.Citizen)
            .Include(u => u.Collector)
                .ThenInclude(c => c != null ? c.Team : null)
            .FirstOrDefaultAsync(u => u.Userid == userId);
    }
}
