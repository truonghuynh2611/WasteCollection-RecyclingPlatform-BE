using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

/// <summary>
/// Waste report repository interface
/// </summary>
public interface IWasteReportRepository : IGenericRepository<WasteReport>
{
    Task<IEnumerable<WasteReport>> GetByCitizenIdAsync(int citizenId);
    Task<IEnumerable<WasteReport>> GetByCollectorIdAsync(int collectorId);
}
