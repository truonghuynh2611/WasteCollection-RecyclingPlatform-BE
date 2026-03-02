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
        private readonly ITeamRepository _teamRepo;
        private readonly ICollectorRepository _collectorRepo;

        public WasteReportService(
            IWasteReportRepository wasteReportRepo,
            ICitizenRepository citizenRepo,
            IAreaRepository areaRepo,
            ITeamRepository teamRepo,
    ICollectorRepository collectorRepo)
        {
            _wasteReportRepo = wasteReportRepo;
            _citizenRepo = citizenRepo;
            _areaRepo = areaRepo;
            _teamRepo = teamRepo;
            _collectorRepo = collectorRepo;
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
                Status = ReportStatus.Pending,
                ExpireTime = DateTime.UtcNow.AddHours(24)
            };


            await _wasteReportRepo.AddAsync(wasteReport);
            await _wasteReportRepo.SaveChangesAsync();

            return wasteReport;
        }
        // ===============================
        // ASSIGN REPORT
        // ===============================

        public async Task AssignReportAsync(int reportId)
        {
            var report = await _wasteReportRepo.GetByIdAsync(reportId);
            if (report == null)
                throw new Exception("Report not found");

            if (report.Status != ReportStatus.Pending)
                throw new Exception("Report is not pending");

            if (report.ExpireTime != null && report.ExpireTime < DateTime.UtcNow)
                throw new Exception("Report expired");

            var primaryTeam = await _teamRepo
                .GetTeamWithCollectorsAsync(report.AreaId, TeamType.PRIMARY);

            if (primaryTeam == null)
                throw new Exception("Primary team not found");

            Team selectedTeam = primaryTeam;

            int totalTasks = primaryTeam.Collectors.Sum(c => c.CurrentTaskCount);

            if (totalTasks >= 5)
            {
                var supportTeam = await _teamRepo
                    .GetTeamWithCollectorsAsync(report.AreaId, TeamType.SUPPORT);

                if (supportTeam != null)
                    selectedTeam = supportTeam;
            }

            var collector = selectedTeam.Collectors
                .Where(c => c.Status)
                .OrderBy(c => c.CurrentTaskCount)
                .FirstOrDefault();

            if (collector == null)
                throw new Exception("No available collector");

            report.CollectorId = collector.CollectorId;
            report.Status = ReportStatus.Assigned;

            collector.CurrentTaskCount++;

            await _wasteReportRepo.UpdateAsync(report);
            await _collectorRepo.UpdateAsync(collector);
            await _wasteReportRepo.SaveChangesAsync();
        }

        // ===============================
        // PROCESS REPORT (COLLECT)
        // ===============================

        public async Task ProcessReportAsync(
            int reportId,
            int collectorId,
            bool isValid,
            string? collectorImageUrl,
            decimal? latitude,
            decimal? longitude)
        {
            var report = await _wasteReportRepo.GetByIdAsync(reportId);
            if (report == null)
                throw new Exception("Report not found");

            if (report.Status != ReportStatus.Assigned)
                throw new Exception("Report is not assigned");

            if (report.CollectorId != collectorId)
                throw new Exception("Not your assigned report");

            var collector = await _collectorRepo.GetByIdAsync(collectorId);
            if (collector == null)
                throw new Exception("Collector not found");

            var citizen = await _citizenRepo.GetByIdAsync(report.CitizenId);
            if (citizen == null)
                throw new Exception("Citizen not found");

            report.CollectorImageUrl = collectorImageUrl;
            report.CollectorLatitude = latitude;
            report.CollectorLongitude = longitude;

            if (isValid)
            {
                report.Status = ReportStatus.Completed;
                citizen.TotalPoints += 10;
            }
            else
            {
                report.Status = ReportStatus.Rejected;
                citizen.TotalPoints -= 5;
                if (citizen.TotalPoints < 0)
                    citizen.TotalPoints = 0;
            }

            if (collector.CurrentTaskCount > 0)
                collector.CurrentTaskCount--;

            await _wasteReportRepo.UpdateAsync(report);
            await _collectorRepo.UpdateAsync(collector);
            await _citizenRepo.UpdateAsync(citizen);

            await _wasteReportRepo.SaveChangesAsync();
        }
        //Delet
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
