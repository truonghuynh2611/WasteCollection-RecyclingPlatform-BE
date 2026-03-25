using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.Business.Services.Implementations;

public class DistrictService : IDistrictService
{
    private readonly IUnitOfWork _unitOfWork;

    public DistrictService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<District>> GetAllAsync()
    {
        return await _unitOfWork.Districts.GetAllAsync();
    }
}
