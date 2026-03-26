using WasteCollectionPlatform.Common.DTOs.Request.Team;
using WasteCollectionPlatform.Common.DTOs.Response.Team;

namespace WasteCollectionPlatform.Business.Services.Interfaces
{
    public interface ITeamService
    {
        Task<IEnumerable<TeamResponseDto>> GetAllTeamsAsync();
        Task<object> CreateTeamAsync(string name, int areaId);
        Task<object> UpdateTeamAsync(int id, string name, int areaId);
        Task DeleteTeamAsync(int id);
        Task AddCollectorToTeamAsync(AddCollectorToTeamDto dto);
        Task RemoveCollectorFromTeamAsync(int teamId, int collectorId);
        Task SetLeaderAsync(int teamId, int collectorId);
        Task RemoveLeaderAsync(int teamId, int collectorId);
        Task<object> CreateCollectorAsync(CreateCollectorDto dto);
        Task<IEnumerable<object>> GetAllCollectorsAsync();
        Task ToggleCollectorStatusAsync(int collectorId);
        Task AssignReportToTeamAsync(int teamId, int reportId);
        Task AssignTeamToAreaAsync(int teamId, int areaId);
    }
}
