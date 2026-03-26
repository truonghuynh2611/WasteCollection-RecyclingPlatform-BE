using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WasteCollectionPlatform.Common.DTOs.Request.Admin;
using WasteCollectionPlatform.Common.DTOs.Request.Collector;

namespace WasteCollectionPlatform.Business.Services.Interfaces
{
    public interface ITeamService
    {
        Task<object> CreateTeamAsync(CreateTeamRequestDto request);

        // Update Team
        Task UpdateTeamAsync(int teamId, UpdateTeamRequestDto request);

        // Delete Team
        Task DeleteTeamAsync(int teamId);

        Task AddCollectorToTeamAsync(AddCollectorToTeamRequestDto request);
        Task<List<CollectorDto>> GetCollectorsByTeamIdAsync(int teamId);
        Task RemoveCollectorFromTeamAsync(RemoveCollectorFromTeamRequestDto request);

        Task<object> CreateCollectorAsync(CreateCollectorRequestDto request);
        Task SetLeaderAsync(int teamId, int collectorId);
        Task RemoveLeaderAsync(int teamId, int collectorId);
        Task<List<CollectorDto>> GetAllCollectorsAsync();
        Task AssignTeamToAreaAsync(int teamId, int areaId);
        Task AssignReportToTeamAsync(AssignReportRequestDto request);
    }
}

