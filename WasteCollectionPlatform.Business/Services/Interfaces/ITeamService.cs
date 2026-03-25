using System.Collections.Generic;
using System.Threading.Tasks;

namespace WasteCollectionPlatform.Business.Services.Interfaces
{
    public interface ITeamService
    {
        Task<IEnumerable<object>> GetAllTeamsAsync();
        Task<object> CreateTeamAsync(string name, int areaId);
        Task AddCollectorToTeamAsync(int teamId, int collectorId);
        Task AssignReportToTeamAsync(int teamId, int reportId);
    }
}
