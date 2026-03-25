using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

/// <summary>
/// Citizen repository interface
/// </summary>
public interface ICitizenRepository : IGenericRepository<Citizen>
{
    Task<Citizen?> GetByUserIdAsync(int userId);
    Task<Citizen?> GetByIdWithDetailsAsync(int citizenId);
}
