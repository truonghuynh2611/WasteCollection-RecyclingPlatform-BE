using WasteCollectionPlatform.Common.Enums;
using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

/// <summary>
/// Team repository interface
/// </summary>
public interface ITeamRepository : IGenericRepository<Team>
{
    Task<Team?> GetByIdWithDetailsAsync(int teamId);
    Task<IEnumerable<Team>> GetByAreaIdAsync(int areaId);
    Task<Team?> GetTeamWithCollectorsAsync(int areaId, TeamType teamType);
}
