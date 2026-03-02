using Microsoft.EntityFrameworkCore;
using WasteReportApp.Models.Dto;
using WasteReportApp.Models.Entities;
using WasteReportApp.Repository;

namespace WasteReportApp.Service
{
    public class WasteReportService : IWasteReportService

    {
        private readonly IWasteReportRepository _wasteReportRepo;
        private readonly ICitizenRepository _citizenRepo;
        private readonly IAreaRepository _areaRepo;

        public WasteReportService(
            IWasteReportRepository wasteReportRepo,
            ICitizenRepository citizenRepo,
            IAreaRepository areaRepo)
        {
            _wasteReportRepo = wasteReportRepo;
            _citizenRepo = citizenRepo;
            _areaRepo = areaRepo;
        }

        public async Task<IEnumerable<WasteReport>> GetAllAsync()
        {
            return await _wasteReportRepo.GetAllAsync();
        }

        public async Task<WasteReport?> GetByIdAsync(int id)
        {
            return await _wasteReportRepo.GetByIdAsync(id);
        }


        public async Task<WasteReport> CreateAsync(CreateWasteReportDto dto)
        {
            // Validate Citizen
            if (!await _citizenRepo.ExistsAsync(dto.CitizenId))
                throw new Exception("Citizen does not exist");

            // Validate Area
            if (!await _areaRepo.ExistsAsync(dto.AreaId))
                throw new Exception("Area does not exist");

            var wasteReport = new WasteReport
            {
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                CitizenId = dto.CitizenId,
                AreaId = dto.AreaId,
                WasteType = dto.WasteType,
                CreatedAt = DateTime.UtcNow,
                Status = ReportStatus.Pending
            };

            await _wasteReportRepo.AddAsync(wasteReport);
            await _wasteReportRepo.SaveChangesAsync();

            return wasteReport;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var report = await _wasteReportRepo.GetByIdAsync(id);
            if (report == null)
                return false;

            await _wasteReportRepo.DeleteAsync(report);
            await _wasteReportRepo.SaveChangesAsync();
            return true;
        }




    }
}
