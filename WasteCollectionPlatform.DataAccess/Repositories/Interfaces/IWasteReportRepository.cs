using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

/// <summary>
/// Waste report repository interface
/// </summary>
public interface IWasteReportRepository : IGenericRepository<Wastereport>
{
    Task<IEnumerable<Wastereport>> GetByCitizenIdAsync(int citizenId);
    Task<IEnumerable<Wastereport>> GetByCollectorIdAsync(int collectorId);
}
