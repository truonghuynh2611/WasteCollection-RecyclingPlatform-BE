using WasteReportApp.Models.Entities;

namespace WasteReportApp.Repository
{
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
}
