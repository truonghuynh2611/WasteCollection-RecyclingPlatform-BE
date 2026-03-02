using WasteReportApp.Models.Entities;

namespace WasteReportApp.Repository
{
    public interface IWasteReportRepository
    {
        Task<IEnumerable<WasteReport>> GetAllAsync();
        Task<WasteReport?> GetByIdAsync(int id);
        Task AddAsync(WasteReport wasteReport);
        Task UpdateAsync(WasteReport wasteReport);
        Task DeleteAsync(WasteReport wasteReport);
        Task SaveChangesAsync();
    }

}

