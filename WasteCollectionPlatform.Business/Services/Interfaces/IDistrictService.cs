using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.Business.Services.Interfaces;

public interface IDistrictService
{
    Task<IEnumerable<District>> GetAllAsync();
}
