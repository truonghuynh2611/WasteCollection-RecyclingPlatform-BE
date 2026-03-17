using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

/// <summary>
/// Waste report repository interface
/// </summary>
public interface IWasteReportRepository : IGenericRepository<WasteReport>
{
    Task<IEnumerable<WasteReport>> GetByCitizenIdAsync(int citizenId);
    Task<IEnumerable<WasteReport>> GetByCollectorIdAsync(int collectorId);
    new Task<IEnumerable<WasteReport>> GetAllAsync();
    new Task<WasteReport?> GetByIdAsync(int id);
    new Task<WasteReport> AddAsync(WasteReport wasteReport);
    new Task UpdateAsync(WasteReport wasteReport);
    new Task DeleteAsync(WasteReport wasteReport);

    Task<List<WasteReport>> GetProcessedReportsAsync();
    Task SaveChangesAsync();
}
