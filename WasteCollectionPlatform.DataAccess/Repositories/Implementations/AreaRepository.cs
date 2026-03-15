using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

public class AreaRepository : IAreaRepository
{
    private readonly WasteManagementContext _context;

    public AreaRepository(WasteManagementContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Area>> GetAllAsync()
    {
        return await _context.Areas
            .Include(a => a.District)
            .Include(a => a.WasteReports)
            .ToListAsync();
    }

    public async Task<Area?> GetByIdAsync(int id)
    {
        return await _context.Areas
            .Include(a => a.District)
            .Include(a => a.WasteReports)
            .FirstOrDefaultAsync(a => a.AreaId == id);
    }

    public async Task AddAsync(Area area)
    {
        await _context.Areas.AddAsync(area);
    }

    public Task UpdateAsync(Area area)
    {
        _context.Areas.Update(area);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Area area)
    {
        _context.Areas.Remove(area);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Areas.AnyAsync(a => a.AreaId == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}