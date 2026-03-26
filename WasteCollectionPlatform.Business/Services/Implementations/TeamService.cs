using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.Enums;
using WasteCollectionPlatform.Common.Exceptions;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.Business.Services.Implementations
{
    public class TeamService : ITeamService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TeamService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<object>> GetAllTeamsAsync()
        {
            var teams = await _unitOfWork.Teams.GetAllAsync();
            var areas = await _unitOfWork.Areas.GetAllAsync();
            var collectors = await _unitOfWork.Collectors.GetAllWithUsersAsync();

            return teams.Select(t => new
            {
                teamId = t.TeamId,
                name = t.Name,
                areaId = t.AreaId,
                areaName = areas.FirstOrDefault(a => a.AreaId == t.AreaId)?.Name,
                collectorCount = collectors.Count(c => c.TeamId == t.TeamId),
                collectors = collectors.Where(c => c.TeamId == t.TeamId)
                                      .Select(c => new { 
                                          c.CollectorId, 
                                          fullName = c.User?.FullName, 
                                          role = (int)c.Role 
                                      }),
                currentTaskCount = t.CurrentTaskCount
            });
        }

        public async Task<object> CreateTeamAsync(string name, int areaId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessRuleException("Tên đội không được để trống");

            var area = await _unitOfWork.Areas.GetByIdAsync(areaId);
            if (area == null)
                throw new BusinessRuleException("Khu vực không tồn tại");

            var team = new Team
            {
                Name = name.Trim(),
                AreaId = areaId,
                CurrentTaskCount = 0
            };

            await _unitOfWork.Teams.AddAsync(team);
            await _unitOfWork.SaveChangesAsync();

            return new { team.TeamId, team.Name, team.AreaId };
        }

        public async Task AddCollectorToTeamAsync(int teamId, int collectorId)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
            if (team == null) throw new BusinessRuleException("Đội không tồn tại");

            var collector = await _unitOfWork.Collectors.GetByIdAsync(collectorId);
            if (collector == null) throw new BusinessRuleException("Nhân viên thu gom không tồn tại");

            collector.TeamId = teamId;
            await _unitOfWork.Collectors.UpdateAsync(collector);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task AssignReportToTeamAsync(int teamId, int reportId)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
            if (team == null) throw new BusinessRuleException("Đội không tồn tại");

            var report = await _unitOfWork.WasteReports.GetByIdAsync(reportId);
            if (report == null) throw new BusinessRuleException("Báo cáo không tồn tại");

            if (report.Status != ReportStatus.Pending)
                throw new BusinessRuleException("Chỉ có thể gán báo cáo đang ở trạng thái Chờ xử lý");

            // Update report
            report.TeamId = teamId;
            report.Status = ReportStatus.Assigned;
            await _unitOfWork.WasteReports.UpdateAsync(report);

            // Create assignment
            var assignment = new ReportAssignment
            {
                ReportId = reportId,
                TeamId = teamId
            };
            await _unitOfWork.ReportAssignments.AddAsync(assignment);

            // Increment team task count
            team.CurrentTaskCount++;
            await _unitOfWork.Teams.UpdateAsync(team);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}