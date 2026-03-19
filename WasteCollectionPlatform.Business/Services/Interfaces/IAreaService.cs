using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WasteCollectionPlatform.Common.DTOs.Request.Admin;

namespace WasteCollectionPlatform.Business.Services.Interfaces
{
    public  interface  IAreaService
    {
        Task<object> CreateAreaAsync(CreateAreaRequestDto request);
        Task<IEnumerable<object>> GetAllAreasAsync();
        Task<object?> GetAreaByIdAsync(int areaId);
        Task UpdateAreaAsync(int areaId, UpdateAreaRequestDto request);
        Task DeleteAreaAsync(int areaId);
    }
}
