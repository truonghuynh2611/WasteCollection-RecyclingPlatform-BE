using System.Linq.Expressions;
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
    Task AddCollectorAsync(int teamId, Collector collector);
    Task<List<Collector>> GetCollectorsByTeamIdAsync(int teamId);
    Task RemoveCollectorAsync(int teamId, int collectorId);
    Task<bool> AnyAsync(Expression<Func<Team, bool>> predicate);
}
