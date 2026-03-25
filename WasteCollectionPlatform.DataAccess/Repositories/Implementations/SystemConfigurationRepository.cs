using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

public class SystemConfigurationRepository : GenericRepository<SystemConfiguration>, ISystemConfigurationRepository
{
    public SystemConfigurationRepository(WasteManagementContext context) : base(context)
    {
    }

    public async Task<SystemConfiguration?> GetByKeyAsync(string key)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Key == key);
    }
}
