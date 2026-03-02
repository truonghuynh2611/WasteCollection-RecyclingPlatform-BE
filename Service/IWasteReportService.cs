using WasteReportApp.Models.Dto;
using WasteReportApp.Models.Entities;

namespace WasteReportApp.Service
{
    public interface IWasteReportService
    {
        Task<IEnumerable<WasteReport>> GetAllAsync();
        Task<WasteReport?> GetByIdAsync(int id);
        Task<WasteReport> CreateAsync(CreateWasteReportDto dto);


        Task<bool> DeleteAsync(int id);



    }
}
