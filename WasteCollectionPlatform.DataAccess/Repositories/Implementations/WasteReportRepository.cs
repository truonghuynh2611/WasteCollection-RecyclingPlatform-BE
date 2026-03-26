using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.Common.DTOs.Request.Admin;
using WasteCollectionPlatform.Common.Enums;
using WasteCollectionPlatform.Common.Exceptions;
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
            .Include(r => r.PointHistories)
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
            .Where(r => r.TeamId == collector.TeamId || r.ReportAssignments.Any(ra => ra.TeamId == collector.TeamId))
            .Include(r => r.Citizen)
            .Include(r => r.ReportImages)
            .Include(r => r.PointHistories)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public override async Task<IEnumerable<WasteReport>> GetAllAsync()
    {
        return await _context.WasteReports
            .Include(w => w.Citizen).ThenInclude(c => c.User)
            .Include(w => w.Area)
            .Include(w => w.ReportImages)
            .Include(w => w.PointHistories)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    public override async Task<WasteReport?> GetByIdAsync(int id)
    {
        return await _context.WasteReports
            .Include(w => w.Citizen).ThenInclude(c => c.User)
            .Include(w => w.Area)
            .Include(w => w.ReportImages)
            .Include(w => w.PointHistories)
            .FirstOrDefaultAsync(w => w.ReportId == id);
    }

    public override async Task<WasteReport> AddAsync(WasteReport wasteReport)
    {
        await _context.WasteReports.AddAsync(wasteReport);
        return wasteReport;
    }

    public async Task CancelReportAsync(CancelReportRequestDto request)
    {
        var report = await GetByIdAsync(request.ReportId);
        if (report == null)
            throw new KeyNotFoundException("Report not found");

        if (report.Status != ReportStatus.Pending)
            throw new BusinessRuleException("Only reports in Pending status can be cancelled");

        report.Status = ReportStatus.Failed;

        await UpdateAsync(report);
        await SaveChangesAsync();
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

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
