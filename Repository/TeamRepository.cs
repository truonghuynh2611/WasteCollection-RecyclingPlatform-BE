using Microsoft.EntityFrameworkCore;
using WasteReportApp.Data;
using WasteReportApp.Models.Entities;

namespace WasteReportApp.Repository
{
    public class TeamRepository : ITeamRepository
    {
        private readonly AppDbContext _context;

        public TeamRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Team?> GetByIdAsync(int id)
        {
            return await _context.Teams
                .Include(t => t.Collectors)
                .FirstOrDefaultAsync(t => t.TeamId == id);
        }

        public async Task<Team?> GetTeamWithCollectorsAsync(int areaId, TeamType type)
        {
            return await _context.Teams
                .Include(t => t.Collectors)
                .FirstOrDefaultAsync(t =>
                    t.AreaId == areaId &&
                    t.TeamType == type);
        }

        public async Task AddAsync(Team team)
        {
            await _context.Teams.AddAsync(team);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
