using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

public interface IAreaRepository
{
    Task<IEnumerable<Area>> GetAllAsync();
    Task<Area?> GetByIdAsync(int id);
    Task AddAsync(Area area);
    Task UpdateAsync(Area area);
    Task DeleteAsync(Area area);
    Task<bool> ExistsAsync(int id);
    Task SaveChangesAsync();
}