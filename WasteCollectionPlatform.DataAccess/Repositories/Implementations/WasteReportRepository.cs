using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.Common.Enums;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.DataAccess.Repositories.Implementations;

public class WasteReportRepository : GenericRepository<WasteReport>, IWasteReportRepository
{
    public WasteReportRepository(WasteManagementContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<WasteReport>> GetByCitizenIdAsync(int citizenId)
    {
        return await _dbSet
            .Where(r => r.CitizenId == citizenId)
            .Include(r => r.ReportImages)
            .Include(r => r.Area)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<WasteReport>> GetByCollectorIdAsync(int collectorId)
    {
        // PostgreSQL schema: Reports are assigned to Teams, not individual collectors
        // Get the collector's team first, then get reports assigned to that team
        var collector = await _context.Collectors
            .FirstOrDefaultAsync(c => c.CollectorId == collectorId);
        
        if (collector == null)
        {
            return new List<WasteReport>();
        }
        
        return await _dbSet
            .Include(r => r.ReportAssignments)
            .Where(r => r.ReportAssignments.Any(ra => ra.TeamId == collector.TeamId))
            .Include(r => r.Citizen)
            .Include(r => r.ReportImages)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public override async Task<IEnumerable<WasteReport>> GetAllAsync()
    {
        return await _context.WasteReports
            .Include(w => w.Citizen)
            .Include(w => w.Area)
            .ToListAsync();
    }

    public override async Task<WasteReport?> GetByIdAsync(int id)
    {
        return await _context.WasteReports
            .Include(w => w.Citizen)
            .Include(w => w.Area)
            .FirstOrDefaultAsync(w => w.ReportId == id);
    }

    public override async Task<WasteReport> AddAsync(WasteReport wasteReport)
    {
        await _context.WasteReports.AddAsync(wasteReport);
        return wasteReport;
    }

    public override Task UpdateAsync(WasteReport wasteReport)
    {
        _context.WasteReports.Update(wasteReport);
        return Task.CompletedTask;
    }

    public override Task DeleteAsync(WasteReport wasteReport)
    {
        _context.WasteReports.Remove(wasteReport);
        return Task.CompletedTask;
    }

    public async Task<List<WasteReport>> GetProcessedReportsAsync()
    {
        return await _context.WasteReports
            .Where(r => r.Status == ReportStatus.Completed || r.Status == ReportStatus.Cancelled)
            .Include(r => r.Citizen)
            .Include(r => r.ReportImages)
            .ToListAsync();
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
