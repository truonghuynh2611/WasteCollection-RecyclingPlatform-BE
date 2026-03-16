using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.Common.Enums;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

/// <summary>
/// Team repository implementation
/// </summary>
public class TeamRepository : GenericRepository<Team>, ITeamRepository
{
    public TeamRepository(WasteManagementContext context) : base(context)
    {
    }
    
    public async Task<Team?> GetByIdWithDetailsAsync(int teamId)
    {
        return await _dbSet
            .Include(t => t.Area)
            .Include(t => t.Collectors)
            .FirstOrDefaultAsync(t => t.TeamId == teamId);
    }
    
    public async Task<IEnumerable<Team>> GetByAreaIdAsync(int areaId)
    {
        return await _dbSet
            .Where(t => t.AreaId == areaId)
            .Include(t => t.Area)
            .Include(t => t.Collectors)
            .ToListAsync();
    }

    public async Task<Team?> GetTeamWithCollectorsAsync(int areaId, TeamType teamType)
    {
        _ = teamType;

        return await _context.Teams
            .Include(t => t.Collectors)
            .Include(t => t.ReportAssignments)
            .FirstOrDefaultAsync(t => t.AreaId == areaId);
    }
}
