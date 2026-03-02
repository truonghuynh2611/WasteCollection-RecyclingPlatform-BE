using WasteReportApp.Models.Entities;

namespace WasteReportApp.Repository
{
    public interface ITeamRepository
    {
        Task<Team?> GetByIdAsync(int id);
        Task<Team?> GetTeamWithCollectorsAsync(int areaId, TeamType type);
        Task AddAsync(Team team);
        Task SaveChangesAsync();
        
    }
}
