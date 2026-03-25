using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

/// <summary>
/// District repository interface
/// </summary>
public interface IDistrictRepository : IGenericRepository<District>
{
    Task<District?> GetByCodeAsync(string code);
    
    /// <summary>
    /// Get district with all areas included
    /// </summary>
    Task<District?> GetDistrictWithAreasAsync(int id);
    
    /// <summary>
    /// Get all districts with areas included
    /// </summary>
    Task<IEnumerable<District>> GetAllDistrictsWithAreasAsync();
}
