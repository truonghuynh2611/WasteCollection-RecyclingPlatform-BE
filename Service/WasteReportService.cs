using Microsoft.EntityFrameworkCore;
using System.Net.Http;
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
        private readonly HttpClient _httpClient;

        public WasteReportService(
            IWasteReportRepository wasteReportRepo,
            ICitizenRepository citizenRepo,
            IAreaRepository areaRepo,
            ITeamRepository teamRepo,
    ICollectorRepository collectorRepo,
    HttpClient httpClient)
        {
            _wasteReportRepo = wasteReportRepo;
            _citizenRepo = citizenRepo;
            _areaRepo = areaRepo;
            _teamRepo = teamRepo;
            _collectorRepo = collectorRepo;
            _httpClient = httpClient;
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
            if (!await IsValidImageUrlAsync(dto.ImageUrl))
                throw new Exception("Invalid image URL. Must be a real image link.");

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

        private async Task<bool> IsValidImageUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            if (uri.Scheme != Uri.UriSchemeHttp &&
                uri.Scheme != Uri.UriSchemeHttps)
                return false;

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, uri);
                using var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return false;

                var contentType = response.Content.Headers.ContentType?.MediaType;

                return contentType != null &&
                       contentType.StartsWith("image/");
            }
            catch
            {
                return false;
            }
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
                throw new Exception("Report must be pending to assign");

            if (report.ExpireTime != null && report.ExpireTime < DateTime.UtcNow)
                throw new Exception("Report expired");

            // 1️⃣ Lấy PRIMARY team
            var primaryTeam = await _teamRepo
                .GetTeamWithCollectorsAsync(report.AreaId, TeamType.PRIMARY);

            if (primaryTeam == null)
                throw new Exception("Primary team not found");

            Team selectedTeam = primaryTeam;

            // 2️⃣ Kiểm tra overload
            bool noActiveCollector = !primaryTeam.Collectors.Any(c => c.Status);

            if (primaryTeam.CurrentTaskCount >= 5 || noActiveCollector)
            {
                var supportTeam = await _teamRepo
                    .GetTeamWithCollectorsAsync(report.AreaId, TeamType.SUPPORT);

                if (supportTeam != null &&
                    supportTeam.Collectors.Any(c => c.Status))
                {
                    selectedTeam = supportTeam;
                }
                else
                {
                    throw new Exception("No available team to assign");
                }
            }

            // 3️⃣ Assign team
            report.TeamId = selectedTeam.TeamId;
            report.Status = ReportStatus.Assigned;

            // 4️⃣ Tăng task cho toàn team được chọn
            selectedTeam.CurrentTaskCount++;

            await _teamRepo.UpdateAsync(selectedTeam);

            await _wasteReportRepo.UpdateAsync(report);
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

            if (report.TeamId == null)
                throw new Exception("Report has no assigned team");

            var collector = await _collectorRepo.GetByIdAsync(collectorId);
            if (collector == null)
                throw new Exception("Collector not found");

            // ❗ Collector phải thuộc team được assign
            if (collector.TeamId != report.TeamId)
                throw new Exception("You are not in the assigned team");

            // ❗ CHỈ LEADER MỚI ĐƯỢC SUBMIT REPORT
            if (collector.Role != CollectorRole.Leader)
                throw new Exception("Only team leader can submit completion report");

            var citizen = await _citizenRepo.GetByIdAsync(report.CitizenId);
            if (citizen == null)
                throw new Exception("Citizen not found");

            if (!string.IsNullOrEmpty(collectorImageUrl))
            {
                if (!await IsValidImageUrlAsync(collectorImageUrl))
                    throw new Exception("Invalid collector image URL.");
            }

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

            // Team hoàn thành task
            var team = await _teamRepo.GetByIdAsync(report.TeamId.Value);

            if (team.CurrentTaskCount > 0)
            {
                team.CurrentTaskCount--;
            }

            await _wasteReportRepo.UpdateAsync(report);
            await _citizenRepo.UpdateAsync(citizen);
            await _teamRepo.UpdateAsync(team);

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
