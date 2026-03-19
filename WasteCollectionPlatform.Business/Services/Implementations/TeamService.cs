using System;
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

namespace WasteCollectionPlatform.Business.Services.Implementations
{
    
public class TeamService : ITeamService
{
    private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamRepository _teamRepository;
        private readonly ICollectorRepository _collectorRepository;

        public TeamService(ITeamRepository teamRepository,
        ICollectorRepository collectorRepository, IUnitOfWork unitOfWork)
    {
            _teamRepository = teamRepository;
            _collectorRepository = collectorRepository;
            _unitOfWork = unitOfWork;
    }

    public async Task<object> CreateTeamAsync(CreateTeamRequestDto request)
    {
        // 🔥 Validate Name
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BusinessRuleException("Tên team không được để trống");
        }

        // 🔥 Check Area tồn tại
        var area = await _unitOfWork.Areas.GetByIdAsync(request.AreaId);
        if (area == null)
        {
            throw new BusinessRuleException("Area không tồn tại");
        }

        // 🔥 Check duplicate trong cùng Area
        var exists = await _unitOfWork.Teams
            .AnyAsync(t => t.AreaId == request.AreaId && t.Name == request.Name);

        if (exists)
        {
            throw new BusinessRuleException("Team đã tồn tại trong Area này");
        }

        // 🔥 Create Team
        var team = new Team
        {
            AreaId = request.AreaId,
            Name = request.Name.Trim(),
            CurrentTaskCount = 0
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
            // 1️⃣ Lấy team
            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
            if (team == null)
                throw new KeyNotFoundException($"Team {teamId} không tồn tại.");

            // 2️⃣ Kiểm tra duplicate Name trong cùng Area
            var exists = await _unitOfWork.Teams.AnyAsync(t => t.TeamId != teamId && t.AreaId == request.AreaId && t.Name == request.Name);
            if (exists)
                throw new BusinessRuleException("Team đã tồn tại trong Area này.");

            // 3️⃣ Update
            team.Name = request.Name.Trim();
            team.AreaId = request.AreaId;

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
            // 1️⃣ Kiểm tra Team tồn tại
            var team = await _teamRepository.GetByIdWithDetailsAsync(request.TeamId);
            if (team == null)
                throw new KeyNotFoundException($"Team với Id {request.TeamId} không tồn tại.");

            // 2️⃣ Kiểm tra Collector tồn tại
            var collector = await _collectorRepository.GetByIdAsync(request.CollectorId);
            if (collector == null)
                throw new KeyNotFoundException($"Collector với Id {request.CollectorId} không tồn tại.");

            // 3️⃣ Kiểm tra Collector đã có trong Team chưa
            if (team.Collectors == null) team.Collectors = new List<Collector>(); // đảm bảo không null
            if (team.Collectors.Any(c => c.CollectorId == request.CollectorId))
                throw new InvalidOperationException("Collector đã thuộc Team này.");

            // 4️⃣ Thêm Collector vào Team
            team.Collectors.Add(collector);

            // 5️⃣ Lưu thay đổi
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<List<CollectorDto>> GetCollectorsByTeamIdAsync(int teamId)
        {
            var collectors = await _teamRepository.GetCollectorsByTeamIdAsync(teamId);

            // Map sang DTO để không lộ entity trực tiếp
            return collectors.Select(c => new CollectorDto
            {
                CollectorId = c.CollectorId
               
                // Thêm các property bạn muốn trả
            }).ToList();
        }
        public async Task RemoveCollectorFromTeamAsync(RemoveCollectorFromTeamRequestDto request)
        {
            // Optional: kiểm tra team tồn tại, collector tồn tại, quyền admin...
            await _teamRepository.RemoveCollectorAsync(request.TeamId, request.CollectorId);
        }
    }
}