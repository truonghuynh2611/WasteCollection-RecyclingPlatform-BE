using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

/// <summary>
/// Point history repository interface
/// </summary>
public interface IPointHistoryRepository : IGenericRepository<PointHistory>
{
    Task<IEnumerable<PointHistory>> GetByCitizenIdAsync(int citizenId);
    Task<IEnumerable<PointHistory>> GetByCitizenIdWithDetailsAsync(int citizenId);
}
