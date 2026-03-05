using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

/// <summary>
/// Collector repository interface
/// </summary>
public interface ICollectorRepository : IGenericRepository<Collector>
{
    Task<Collector?> GetByUserIdAsync(int userId);
    Task<Collector?> GetByIdWithDetailsAsync(int collectorId);
}
