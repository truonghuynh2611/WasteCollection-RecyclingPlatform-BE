using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

/// <summary>
/// Repository implementation for Enterprise entity
/// </summary>
public class EnterpriseRepository : GenericRepository<Enterprise>, IEnterpriseRepository
{
    public EnterpriseRepository(WasteManagementContext context) : base(context)
    {
    }

    public async Task<Enterprise?> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(e => e.User)
            .Include(e => e.District)
            .FirstOrDefaultAsync(e => e.Userid == userId);
    }
}
