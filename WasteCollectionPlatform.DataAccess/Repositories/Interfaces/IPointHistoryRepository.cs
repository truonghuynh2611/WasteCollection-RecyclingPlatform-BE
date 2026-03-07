using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

/// <summary>
/// Point history repository interface
/// </summary>
public interface IPointHistoryRepository : IGenericRepository<Pointhistory>
{
    Task<IEnumerable<Pointhistory>> GetByCitizenIdAsync(int citizenId);
}
