using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

public interface ISystemConfigurationRepository : IGenericRepository<SystemConfiguration>
{
    Task<SystemConfiguration?> GetByKeyAsync(string key);
}
