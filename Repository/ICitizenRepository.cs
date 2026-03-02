using WasteReportApp.Models.Entities;

namespace WasteReportApp.Repository
{
    public interface ICitizenRepository
    {
        Task<IEnumerable<Citizen>> GetAllAsync();
        Task<Citizen?> GetByIdAsync(int id);
        Task AddAsync(Citizen citizen);
        Task UpdateAsync(Citizen citizen);
        Task DeleteAsync(Citizen citizen);
        Task<bool> ExistsAsync(int id);
        Task SaveChangesAsync();


    }
}
