using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

/// <summary>
/// Repository interface for Enterprise entity
/// </summary>
public interface IEnterpriseRepository : IGenericRepository<Enterprise>
{
    /// <summary>
    /// Get enterprise by user ID
    /// </summary>
    Task<Enterprise?> GetByUserIdAsync(int userId);
}
