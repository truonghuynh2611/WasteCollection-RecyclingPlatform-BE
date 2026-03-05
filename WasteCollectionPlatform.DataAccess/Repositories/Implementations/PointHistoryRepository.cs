using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

public class PointHistoryRepository : GenericRepository<Pointhistory>, IPointHistoryRepository
{
    public PointHistoryRepository(WasteManagementContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<Pointhistory>> GetByCitizenIdAsync(int citizenId)
    {
        return await _dbSet
            .Where(p => p.Citizenid == citizenId)
            .OrderByDescending(p => p.Createdat)
            .ToListAsync();
    }
}
