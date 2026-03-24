using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

public class PendingRegistrationRepository : GenericRepository<PendingRegistration>, IPendingRegistrationRepository
{
    public PendingRegistrationRepository(WasteManagementContext context) : base(context)
    {
    }

    public async Task<PendingRegistration?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Email == email);
    }

    public async Task<PendingRegistration?> GetByCodeAsync(string email, string code)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Email == email && p.VerificationCode == code);
    }
}
