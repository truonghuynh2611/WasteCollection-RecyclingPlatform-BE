using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

public class PointHistoryRepository : GenericRepository<PointHistory>, IPointHistoryRepository
{
    public PointHistoryRepository(WasteManagementContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<PointHistory>> GetByCitizenIdAsync(int citizenId)
    {
        return await _dbSet
            .Where(p => p.CitizenId == citizenId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PointHistory>> GetByCitizenIdWithDetailsAsync(int citizenId)
    {
        return await _dbSet
            .Include(p => p.Voucher)
            .Where(p => p.CitizenId == citizenId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}
