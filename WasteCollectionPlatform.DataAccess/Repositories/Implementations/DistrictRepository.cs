using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

public class DistrictRepository : GenericRepository<District>, IDistrictRepository
{
    public DistrictRepository(WasteManagementContext context) : base(context)
    {
    }
    
    public async Task<District?> GetByCodeAsync(string code)
    {
        // PostgreSQL District has no Code field, using Districtname instead
        return await _dbSet
            .FirstOrDefaultAsync(d => d.Districtname.ToLower() == code.ToLower());
    }
    
    public async Task<District?> GetDistrictWithAreasAsync(int id)
    {
        return await _dbSet
            .Include(d => d.Areas)
            .FirstOrDefaultAsync(d => d.Districtid == id);
    }
    
    public async Task<IEnumerable<District>> GetAllDistrictsWithAreasAsync()
    {
        return await _dbSet
            .Include(d => d.Areas)
            .OrderBy(d => d.Districtid)
            .ToListAsync();
    }
}
