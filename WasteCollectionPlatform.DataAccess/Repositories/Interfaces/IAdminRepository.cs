using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

/// <summary>
/// Admin repository interface
/// </summary>
public interface IAdminRepository : IGenericRepository<Admin>
{
    Task<Admin?> GetByUserIdAsync(int userId);
    Task<Admin?> GetByIdWithDetailsAsync(int adminId);
    Task<List<Admin>> GetAllWithDetailsAsync();
    Task<bool> UserIsAdminAsync(int userId);
}
