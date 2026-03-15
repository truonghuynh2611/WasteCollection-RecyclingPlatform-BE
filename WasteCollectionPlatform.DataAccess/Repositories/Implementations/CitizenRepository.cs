using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

/// <summary>
/// Citizen repository implementation
/// </summary>
public class CitizenRepository : GenericRepository<Citizen>, ICitizenRepository
{
    public CitizenRepository(WasteManagementContext context) : base(context)
    {
    }
    
    public async Task<Citizen?> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Userid == userId);
    }
    
    public async Task<Citizen?> GetByIdWithDetailsAsync(int citizenId)
    {
        return await _dbSet
            .Include(c => c.User)
            .Include(c => c.WasteReports)
            .Include(c => c.PointHistories)
            .FirstOrDefaultAsync(c => c.Citizenid == citizenId);
    }
}
