using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

/// <summary>
/// Collector repository implementation
/// </summary>
public class CollectorRepository : GenericRepository<Collector>, ICollectorRepository
{
    public CollectorRepository(WasteManagementContext context) : base(context)
    {
    }
    
    public async Task<Collector?> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }
    
    public async Task<Collector?> GetByIdWithDetailsAsync(int collectorId)
    {
        return await _dbSet
            .Include(c => c.User)
            .Include(c => c.Team)
            .FirstOrDefaultAsync(c => c.CollectorId == collectorId);
    }
}
