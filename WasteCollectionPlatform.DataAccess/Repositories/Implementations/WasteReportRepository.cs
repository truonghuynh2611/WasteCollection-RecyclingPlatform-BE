using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

public class WasteReportRepository : GenericRepository<Wastereport>, IWasteReportRepository
{
    public WasteReportRepository(WasteManagementContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<Wastereport>> GetByCitizenIdAsync(int citizenId)
    {
        return await _dbSet
            .Where(r => r.Citizenid == citizenId)
            .Include(r => r.Reportimages)
            .Include(r => r.Area)
            .OrderByDescending(r => r.Createdat)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Wastereport>> GetByCollectorIdAsync(int collectorId)
    {
        // PostgreSQL schema: Reports are assigned to Teams, not individual collectors
        // Get the collector's team first, then get reports assigned to that team
        var collector = await _context.Collectors
            .FirstOrDefaultAsync(c => c.Collectorid == collectorId);
        
        if (collector == null)
        {
            return new List<Wastereport>();
        }
        
        return await _dbSet
            .Include(r => r.Reportassignments)
            .Where(r => r.Reportassignments.Any(ra => ra.TeamId == collector.TeamId))
            .Include(r => r.Citizen)
            .Include(r => r.Reportimages)
            .OrderByDescending(r => r.Createdat)
            .ToListAsync();
    }
}
