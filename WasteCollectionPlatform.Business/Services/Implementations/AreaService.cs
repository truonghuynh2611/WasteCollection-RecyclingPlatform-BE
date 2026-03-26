using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Request.Admin;
using WasteCollectionPlatform.Common.Exceptions;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.Business.Services.Implementations
{
    public class AreaService : IAreaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AreaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<object> CreateAreaAsync(CreateAreaRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BusinessRuleException("Name không được để trống");
            }

            var district = await _unitOfWork.Districts.GetByIdAsync(request.DistrictId);
            if (district == null)
            {
                throw new BusinessRuleException("District không tồn tại");
            }

            var exists = await _unitOfWork.Areas
                .AnyAsync(x => x.DistrictId == request.DistrictId && x.Name == request.Name);

            if (exists)
            {
                throw new BusinessRuleException("Area đã tồn tại");
            }

            var area = new Area
            {
                DistrictId = request.DistrictId,
                Name = request.Name.Trim()
            };

            await _unitOfWork.Areas.AddAsync(area);
            await _unitOfWork.SaveChangesAsync();

            return new
            {
                area.AreaId,
                area.Name,
                area.DistrictId
            };
        }
        public async Task<IEnumerable<object>> GetAllAreasAsync()
        {
            // We use UnitOfWork but for simplicity we can just get all and then map
            // In a real app we'd use .Include() in the repository or a specialized query
            var areas = await _unitOfWork.Areas.GetAllAsync();
            var districts = await _unitOfWork.Districts.GetAllAsync();
            var teams = await _unitOfWork.Teams.GetAllAsync();
            
            return areas.Select(a => {
                var district = districts.FirstOrDefault(d => d.DistrictId == a.DistrictId);
                var areaTeams = teams.Where(t => t.AreaId == a.AreaId).ToList();
                
                return new {
                    areaId = a.AreaId,
                    name = a.Name,
                    districtId = a.DistrictId,
                    district = district != null ? new {
                        districtId = district.DistrictId,
                        districtName = district.DistrictName
                    } : null,
                    teamCount = areaTeams.Count,
                    teams = areaTeams.Select(t => new {
                        teamId = t.TeamId,
                        name = t.Name,
                        areaId = t.AreaId,
                        type = (int)t.Type
                    }).ToList(),
                    totalReports = 0 // Placeholder or fetch from waste reports
                };
            });
        }

        public async Task<object?> GetAreaByIdAsync(int areaId)
        {
            var area = await _unitOfWork.Areas.GetByIdAsync(areaId);
            if (area == null) return null;
            var teams = await _unitOfWork.Teams.GetAllAsync();
            var areaTeams = teams.Where(t => t.AreaId == areaId).ToList();

            return new { 
                area.AreaId, 
                area.Name, 
                area.DistrictId,
                teams = areaTeams.Select(t => new {
                    teamId = t.TeamId,
                    name = t.Name,
                    areaId = t.AreaId,
                    type = (int)t.Type
                }).ToList()
            };
        }

        public async Task UpdateAreaAsync(int areaId, UpdateAreaRequestDto request)
        {
            var area = await _unitOfWork.Areas.GetByIdAsync(areaId);
            if (area == null)
                throw new BusinessRuleException("Area không tồn tại");

            // Check duplicate name trong cùng district
            var exists = await _unitOfWork.Areas.AnyAsync(x =>
                x.AreaId != areaId &&
                x.DistrictId == request.DistrictId &&
                x.Name == request.Name);
            if (exists)
                throw new BusinessRuleException("Area đã tồn tại trong District này");

            area.Name = request.Name.Trim();
            area.DistrictId = request.DistrictId;

            await _unitOfWork.Areas.UpdateAsync(area);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAreaAsync(int areaId)
        {
            var area = await _unitOfWork.Areas.GetByIdAsync(areaId);
            if (area == null)
                throw new BusinessRuleException("Area không tồn tại");

            await _unitOfWork.Areas.DeleteAsync(area);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
