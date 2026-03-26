using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.Enums;
using WasteCollectionPlatform.Common.Exceptions;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Request.Team;

using WasteCollectionPlatform.Common.DTOs.Response.Team;

namespace WasteCollectionPlatform.Business.Services.Implementations
{
    public class TeamService : ITeamService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TeamService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TeamResponseDto>> GetAllTeamsAsync()
        {
            var teams = await _unitOfWork.Teams.GetAllAsync();
            var areas = await _unitOfWork.Areas.GetAllAsync();
            var collectors = await _unitOfWork.Collectors.GetAllWithUsersAsync();

            return teams.Select(t => new TeamResponseDto
            {
                TeamId = t.TeamId,
                Name = t.Name,
                AreaId = t.AreaId,
                AreaName = areas.FirstOrDefault(a => a.AreaId == t.AreaId)?.Name,
                CollectorCount = collectors.Count(c => c.TeamId == t.TeamId),
                Collectors = collectors.Where(c => c.TeamId == t.TeamId)
                                       .Select(c => new TeamCollectorDto
                                       { 
                                           CollectorId = c.CollectorId, 
                                           FullName = c.User?.FullName, 
                                           Role = c.Role.ToString() 
                                       }),
                CurrentTaskCount = t.CurrentTaskCount
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

        public async Task<object> UpdateTeamAsync(int id, string name, int areaId)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(id);
            if (team == null) throw new BusinessRuleException("Đội không tồn tại");

            team.Name = name.Trim();
            team.AreaId = areaId;

            await _unitOfWork.Teams.UpdateAsync(team);
            await _unitOfWork.SaveChangesAsync();

            return new { team.TeamId, team.Name, team.AreaId };
        }

        public async Task DeleteTeamAsync(int id)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(id);
            if (team == null) return;

            // Clear collectors from team
            var collectors = (await _unitOfWork.Collectors.GetAllAsync()).Where(c => c.TeamId == id);
            foreach (var c in collectors)
            {
                c.TeamId = null; // Correctly allow collector to exist without a team
                await _unitOfWork.Collectors.UpdateAsync(c);
            }

            await _unitOfWork.Teams.DeleteAsync(team);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task AddCollectorToTeamAsync(AddCollectorToTeamDto dto)
        {
            var teamId = dto.TeamId;
            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
            if (team == null) throw new BusinessRuleException("Đội không tồn tại");

            var collector = await _unitOfWork.Collectors.GetByIdAsync(dto.CollectorId);
            if (collector == null) throw new BusinessRuleException("Nhân viên thu gom không tồn tại");

            var collectorsInTeam = (await _unitOfWork.Collectors.GetAllAsync()).Where(c => c.TeamId == teamId).ToList();
            
            // If the collector is NOT currently in this team, check the 3-person limit
            if (collector.TeamId != teamId && collectorsInTeam.Count >= 3)
                throw new BusinessRuleException("Đội đã đủ số lượng (tối đa 3 người)");

            // If assigning as Leader, demote ANY existing leader in this team first (consistent with SetLeaderAsync)
            if (dto.Role == CollectorRole.Leader)
            {
                foreach (var c in collectorsInTeam.Where(c => c.Role == CollectorRole.Leader && c.CollectorId != dto.CollectorId))
                {
                    c.Role = CollectorRole.Member;
                    await _unitOfWork.Collectors.UpdateAsync(c);
                }
            }

            collector.TeamId = teamId;
            collector.Role = dto.Role;
            
            await _unitOfWork.Collectors.UpdateAsync(collector);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveCollectorFromTeamAsync(int teamId, int collectorId)
        {
            var collector = await _unitOfWork.Collectors.GetByIdAsync(collectorId);
            if (collector == null || collector.TeamId != teamId) return;

            collector.TeamId = null; // Correctly allow collector to exist without a team
            collector.Role = CollectorRole.Member;
            await _unitOfWork.Collectors.UpdateAsync(collector);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task SetLeaderAsync(int teamId, int collectorId)
        {
            var collectorsInTeam = (await _unitOfWork.Collectors.GetAllAsync()).Where(c => c.TeamId == teamId).ToList();
            var collector = collectorsInTeam.FirstOrDefault(c => c.CollectorId == collectorId);
            if (collector == null) throw new BusinessRuleException("Nhân viên không thuộc đội này");

            // Reset current leader if any
            foreach (var c in collectorsInTeam.Where(c => c.Role == CollectorRole.Leader))
            {
                c.Role = CollectorRole.Member;
                await _unitOfWork.Collectors.UpdateAsync(c);
            }

            collector.Role = CollectorRole.Leader;
            await _unitOfWork.Collectors.UpdateAsync(collector);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveLeaderAsync(int teamId, int collectorId)
        {
            var collector = await _unitOfWork.Collectors.GetByIdAsync(collectorId);
            if (collector == null || collector.TeamId != teamId) return;

            collector.Role = CollectorRole.Member;
            await _unitOfWork.Collectors.UpdateAsync(collector);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<object> CreateCollectorAsync(CreateCollectorDto dto)
        {
            // 1. Check if email already exists
            if (await _unitOfWork.Users.EmailExistsAsync(dto.Email))
            {
                throw new BusinessRuleException(Common.Constants.ErrorMessages.EmailAlreadyExists);
            }

            // 2. Check team capacity
            var team = await _unitOfWork.Teams.GetByIdAsync(dto.TeamId);
            if (team == null) throw new BusinessRuleException("Đội không tồn tại");

            var collectorsInTeam = (await _unitOfWork.Collectors.GetAllAsync()).Where(c => c.TeamId == dto.TeamId).ToList();
            if (collectorsInTeam.Count >= 3)
                throw new BusinessRuleException("Đội đã đủ số lượng (tối đa 3 người)");

            // 3. Create User
            var user = new DataAccess.Entities.User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Password = Common.Helpers.PasswordHasher.HashPassword(dto.Password),
                Phone = dto.Phone,
                Role = UserRole.Collector,
                Status = true,
                EmailVerified = true // Auto-verified since created by Admin
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync(); // To get UserId

            // 4. Create Collector Profile
            var collector = new DataAccess.Entities.Collector
            {
                UserId = user.UserId,
                TeamId = dto.TeamId,
                Role = CollectorRole.Member,
                Status = true
            };

            await _unitOfWork.Collectors.AddAsync(collector);
            await _unitOfWork.SaveChangesAsync();

            return new
            {
                collector.CollectorId,
                user.UserId,
                user.FullName,
                user.Email,
                collector.TeamId,
                Role = collector.Role.ToString(),
                collector.Status
            };
        }

        public async Task<IEnumerable<object>> GetAllCollectorsAsync()
        {
            var collectors = await _unitOfWork.Collectors.GetAllWithUsersAsync();
            return collectors.Select(c => new
            {
                c.CollectorId,
                userId = c.UserId,
                fullName = c.User?.FullName,
                email = c.User?.Email,
                phone = c.User?.Phone,
                TeamId = c.TeamId,
                role = c.Role.ToString(),
                status = c.Status ?? c.User?.Status ?? false,
                rating = 4.5 // Mock rating for UI display
            });
        }

        public async Task ToggleCollectorStatusAsync(int collectorId)
        {
            var collector = await _unitOfWork.Collectors.GetByIdAsync(collectorId);
            if (collector == null) throw new BusinessRuleException("Người thu gom không tồn tại");

            var user = await _unitOfWork.Users.GetByIdAsync(collector.UserId);
            if (user != null)
            {
                user.Status = !(user.Status ?? true);
                await _unitOfWork.Users.UpdateAsync(user);
            }
            
            collector.Status = !(collector.Status ?? true);
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

            if (team.AreaId != report.AreaId)
                throw new BusinessRuleException("Đội này không thuộc khu vực của báo cáo rác này");

            if (team.CurrentTaskCount >= 20)
                throw new BusinessRuleException("Đội này đã đạt giới hạn tối đa 20 đơn, không thể nhận thêm");

            report.TeamId = teamId;
            report.Status = ReportStatus.Assigned;
            await _unitOfWork.WasteReports.UpdateAsync(report);

            var assignment = new ReportAssignment { ReportId = reportId, TeamId = teamId };
            await _unitOfWork.ReportAssignments.AddAsync(assignment);

            team.CurrentTaskCount++;
            await _unitOfWork.Teams.UpdateAsync(team);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task AssignTeamToAreaAsync(int teamId, int areaId)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
            if (team == null) throw new BusinessRuleException("Đội không tồn tại");

            var area = await _unitOfWork.Areas.GetByIdAsync(areaId);
            if (area == null) throw new BusinessRuleException("Khu vực không tồn tại");

            team.AreaId = areaId;
            await _unitOfWork.Teams.UpdateAsync(team);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}