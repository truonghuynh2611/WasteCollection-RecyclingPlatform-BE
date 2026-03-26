using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Request.Admin;
using WasteCollectionPlatform.Common.Exceptions;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Implementations;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Request.Collector;
using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.Business.Services.Implementations
{
    public class TeamService : ITeamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamRepository _teamRepository;
        private readonly ICollectorRepository _collectorRepository;

        public TeamService(ITeamRepository teamRepository, ICollectorRepository collectorRepository, IUnitOfWork unitOfWork)
        {
            _teamRepository = teamRepository;
            _collectorRepository = collectorRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<object> CreateTeamAsync(CreateTeamRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BusinessRuleException("Tên team không được để trống");
            }

            var area = await _unitOfWork.Areas.GetByIdAsync(request.AreaId);
            if (area == null)
            {
                throw new BusinessRuleException("Area không tồn tại");
            }

            var exists = await _unitOfWork.Teams.ExistsAsync(t => t.AreaId == request.AreaId && t.Name == request.Name);
            if (exists)
            {
                throw new BusinessRuleException("Team đã tồn tại trong Area này");
            }

            // Kiểm tra giới hạn: 1 Đội chính và 1 Đội hỗ trợ mỗi khu vực
            var areaTeams = await _unitOfWork.Teams.GetByAreaIdAsync(request.AreaId);
            if (request.Type == TeamType.Main)
            {
                if (areaTeams.Any(t => t.Type == TeamType.Main))
                {
                    throw new BusinessRuleException("Khu vực này đã có đội chính.");
                }
            }
            else if (request.Type == TeamType.Support)
            {
                if (areaTeams.Any(t => t.Type == TeamType.Support))
                {
                    throw new BusinessRuleException("Khu vực này đã có đội hỗ trợ.");
                }
            }
            var team = new Team
            {
                AreaId = request.AreaId,
                Name = request.Name.Trim(),
                CurrentTaskCount = 0,
                Type = request.Type
            };

            await _unitOfWork.Teams.AddAsync(team);
            await _unitOfWork.SaveChangesAsync();

            return new
            {
                team.TeamId,
                team.Name,
                team.AreaId,
                team.CurrentTaskCount
            };
        }

        public async Task UpdateTeamAsync(int teamId, UpdateTeamRequestDto request)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
            if (team == null)
                throw new KeyNotFoundException($"Team {teamId} không tồn tại.");

            // Nếu gán vào area mới, kiểm tra trùng tên trong area đó
            if (request.AreaId.HasValue)
            {
                var exists = await _unitOfWork.Teams.ExistsAsync(t => 
                    t.TeamId != teamId && 
                    t.AreaId == request.AreaId && 
                    t.Name == request.Name);
                if (exists)
                    throw new BusinessRuleException("Team đã tồn tại trong Area này.");
            }

            team.Name = request.Name.Trim();
            team.AreaId = request.AreaId;
            if (request.Type.HasValue)
            {
                var areaTeams = await _unitOfWork.Teams.GetByAreaIdAsync(team.AreaId.Value);
                if (request.Type.Value == TeamType.Main && team.Type != TeamType.Main)
                {
                    if (areaTeams.Any(t => t.Type == TeamType.Main && t.TeamId != teamId))
                    {
                        throw new BusinessRuleException("Khu vực này đã có đội chính.");
                    }
                }
                else if (request.Type.Value == TeamType.Support && team.Type != TeamType.Support)
                {
                    if (areaTeams.Any(t => t.Type == TeamType.Support && t.TeamId != teamId))
                    {
                        throw new BusinessRuleException("Khu vực này đã có đội hỗ trợ.");
                    }
                }
                team.Type = request.Type.Value;
            }

            await _unitOfWork.Teams.UpdateAsync(team);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteTeamAsync(int teamId)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
            if (team == null)
                throw new KeyNotFoundException($"Team {teamId} không tồn tại.");

            await _unitOfWork.Teams.DeleteAsync(team);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task AddCollectorToTeamAsync(AddCollectorToTeamRequestDto request)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(request.TeamId);
            if (team == null)
                throw new KeyNotFoundException($"Team với Id {request.TeamId} không tồn tại.");

            var collector = await _unitOfWork.Collectors.GetByIdAsync(request.CollectorId);
            if (collector == null)
                throw new KeyNotFoundException($"Collector với Id {request.CollectorId} không tồn tại.");

            if (collector.TeamId != null && collector.TeamId != request.TeamId)
                throw new BusinessRuleException("Nhân viên này đã thuộc một đội khác. Vui lòng gỡ khỏi đội cũ trước.");

            if (collector.TeamId == request.TeamId)
                throw new BusinessRuleException("Nhân viên đã thuộc đội này.");

            collector.TeamId = request.TeamId;
            collector.Role = CollectorRole.Member;

            await _unitOfWork.Collectors.UpdateAsync(collector);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<CollectorDto>> GetCollectorsByTeamIdAsync(int teamId)
        {
            var collectors = await _teamRepository.GetCollectorsByTeamIdAsync(teamId);

            return collectors.Select(c => new CollectorDto
            {
                CollectorId = c.CollectorId,
                UserId = c.UserId,
                FullName = c.User?.FullName ?? "Unknown",
                Email = c.User?.Email ?? "Unknown",
                Role = c.Role.ToString(),
                Status = c.Status
            }).ToList();
        }

        public async Task RemoveCollectorFromTeamAsync(RemoveCollectorFromTeamRequestDto request)
        {
            await _teamRepository.RemoveCollectorAsync(request.TeamId, request.CollectorId);
        }

        public async Task<object> CreateCollectorAsync(CreateCollectorRequestDto request)
        {
            var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
            if (existingUser != null)
                throw new BusinessRuleException("Email đã được sử dụng.");

            if (request.TeamId.HasValue)
            {
                var team = await _unitOfWork.Teams.GetByIdAsync(request.TeamId.Value);
                if (team == null)
                    throw new BusinessRuleException("Team không tồn tại.");
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Password = WasteCollectionPlatform.Common.Helpers.PasswordHasher.HashPassword(request.Password),
                Phone = request.Phone,
                Role = UserRole.Collector,
                Status = true,
                EmailVerified = true
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var collector = new Collector
            {
                UserId = user.UserId,
                TeamId = request.TeamId,
                Role = request.Role,
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
                collector.Role,
                collector.TeamId
            };
        }

        public async Task SetLeaderAsync(int teamId, int collectorId)
        {
            var team = await _teamRepository.GetByIdWithDetailsAsync(teamId);
            if (team == null)
                throw new KeyNotFoundException("Team không tồn tại.");

            var collector = await _collectorRepository.GetByIdAsync(collectorId);
            if (collector == null || collector.TeamId != teamId)
                throw new BusinessRuleException("Collector không thuộc Team này.");

            foreach (var member in team.Collectors.Where(c => c.Role == CollectorRole.Leader))
            {
                member.Role = CollectorRole.Member;
                await _collectorRepository.UpdateAsync(member);
            }

            collector.Role = CollectorRole.Leader;
            await _collectorRepository.UpdateAsync(collector);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveLeaderAsync(int teamId, int collectorId)
        {
            var collector = await _collectorRepository.GetByIdAsync(collectorId);
            if (collector == null || collector.TeamId != teamId)
                throw new BusinessRuleException("Collector không thuộc Team này.");

            collector.Role = CollectorRole.Member;
            await _collectorRepository.UpdateAsync(collector);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<CollectorDto>> GetAllCollectorsAsync()
        {
            var collectors = await _collectorRepository.GetAllAsync();
            var result = new List<CollectorDto>();

            foreach (var c in collectors)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(c.UserId);
                if (user == null) continue;

                result.Add(new CollectorDto
                {
                    CollectorId = c.CollectorId,
                    UserId = c.UserId,
                    TeamId = c.TeamId,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = c.Role.ToString(),
                    Status = user.Status
                });
            }

            return result;
        }

        public async Task AssignTeamToAreaAsync(int teamId, int areaId)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
            if (team == null)
                throw new KeyNotFoundException($"Team {teamId} không tồn tại.");

            var area = await _unitOfWork.Areas.GetByIdAsync(areaId);
            if (area == null)
                throw new KeyNotFoundException($"Area {areaId} không tồn tại.");

            var areaTeams = await _unitOfWork.Teams.GetByAreaIdAsync(areaId);
            if (team.Type == TeamType.Main)
            {
                if (areaTeams.Any(t => t.Type == TeamType.Main && t.TeamId != teamId))
                {
                    throw new BusinessRuleException("Khu vực này đã có đội chính.");
                }
            }
            else if (team.Type == TeamType.Support)
            {
                if (areaTeams.Any(t => t.Type == TeamType.Support && t.TeamId != teamId))
                {
                    throw new BusinessRuleException("Khu vực này đã có đội hỗ trợ.");
                }
            }
            team.AreaId = areaId;
            await _unitOfWork.Teams.UpdateAsync(team);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}