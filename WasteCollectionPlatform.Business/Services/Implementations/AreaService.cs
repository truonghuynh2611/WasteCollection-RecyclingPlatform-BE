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
            var areas = await _unitOfWork.Areas.GetAllAsync();
            return areas.Select(a => new { a.AreaId, a.Name, a.DistrictId });
        }

        public async Task<object?> GetAreaByIdAsync(int areaId)
        {
            var area = await _unitOfWork.Areas.GetByIdAsync(areaId);
            if (area == null) return null;
            return new { area.AreaId, area.Name, area.DistrictId };
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
