using System.Collections.Generic;
using System.Threading.Tasks;
using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

/// <summary>
/// Voucher repository interface
/// </summary>
public interface IVoucherRepository : IGenericRepository<Voucher>
{
    Task<IEnumerable<Voucher>> GetByCitizenIdAsync(int citizenId);
    Task SaveChangesAsync();
}
