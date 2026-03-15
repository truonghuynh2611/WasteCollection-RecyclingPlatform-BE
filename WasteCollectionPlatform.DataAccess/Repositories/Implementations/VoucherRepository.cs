using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

public class VoucherRepository : GenericRepository<Voucher>, IVoucherRepository
{
    public VoucherRepository(WasteManagementContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<Voucher>> GetByCitizenIdAsync(int citizenId)
    {
        // PostgreSQL schema: Vouchers are linked to citizens via Pointhistory
        // Get vouchers that this citizen has redeemed
        var pointHistories = await _context.PointHistories
            .Where(ph => ph.Citizenid == citizenId && ph.Voucherid != null)
            .Include(ph => ph.Voucher)
            .OrderByDescending(ph => ph.Createdat)
            .ToListAsync();
        
        return pointHistories
            .Where(ph => ph.Voucher != null)
            .Select(ph => ph.Voucher!)
            .ToList();
    }
}
