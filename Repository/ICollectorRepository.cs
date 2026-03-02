using WasteReportApp.Models.Entities;

namespace WasteReportApp.Repository
{
    public interface ICollectorRepository
    {
        Task<Collector?> GetByIdAsync(int id);
        Task UpdateAsync(Collector collector);
        Task SaveChangesAsync();
    }
}
