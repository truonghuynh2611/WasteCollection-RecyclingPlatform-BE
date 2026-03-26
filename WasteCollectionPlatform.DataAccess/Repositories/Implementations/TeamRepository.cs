using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
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
            .Include(t => t.Collectors).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(t => t.TeamId == teamId);
    }
    
    public async Task<IEnumerable<Team>> GetByAreaIdAsync(int areaId)
    {
        return await _dbSet
            .Where(t => t.AreaId == areaId)
            .Include(t => t.Area)
            .Include(t => t.Collectors).ThenInclude(c => c.User)
            .ToListAsync();
    }

    public async Task<Team?> GetTeamWithCollectorsAsync(int areaId, TeamType teamType)
    {
        return await _context.Teams
            .Include(t => t.Collectors).ThenInclude(c => c.User)
            .Include(t => t.ReportAssignments)
            .FirstOrDefaultAsync(t => t.AreaId == areaId && t.Type == teamType);
    }
    public async Task<bool> AnyAsync(Expression<Func<Team, bool>> predicate)
    {
        return await _context.Teams.AnyAsync(predicate); // ?? EF Core AnyAsync
    }
    public async Task AddCollectorAsync(int teamId, Collector collector)
    {
        var team = await _dbSet
            .Include(t => t.Collectors)
            .FirstOrDefaultAsync(t => t.TeamId == teamId);

        if (team == null)
            throw new KeyNotFoundException($"Team with Id {teamId} not found.");

        team.Collectors.Add(collector);
    }
    public async Task<List<Collector>> GetCollectorsByTeamIdAsync(int teamId)
    {
        var team = await _context.Teams
            .Include(t => t.Collectors).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(t => t.TeamId == teamId);

        if (team == null)
            throw new KeyNotFoundException("Team not found");

        return team.Collectors.ToList();
    }
    public async Task RemoveCollectorAsync(int teamId, int collectorId)
    {
        var team = await _context.Teams
            .Include(t => t.Collectors)
            .FirstOrDefaultAsync(t => t.TeamId == teamId);

        if (team == null)
            throw new KeyNotFoundException("Team not found");

        var collector = team.Collectors.FirstOrDefault(c => c.CollectorId == collectorId);
        if (collector == null)
            throw new KeyNotFoundException("Collector not found in this team");

        team.Collectors.Remove(collector);

        await _context.SaveChangesAsync();
    }
}
