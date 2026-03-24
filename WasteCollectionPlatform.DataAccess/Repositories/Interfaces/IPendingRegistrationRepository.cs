using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

public interface IPendingRegistrationRepository : IGenericRepository<PendingRegistration>
{
    Task<PendingRegistration?> GetByEmailAsync(string email);
    Task<PendingRegistration?> GetByCodeAsync(string email, string code);
}
