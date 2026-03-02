using Microsoft.EntityFrameworkCore;
using WasteReportApp.Data;
using WasteReportApp.Models.Entities;

namespace WasteReportApp.Repository
{
    public class WasteReportRepository : IWasteReportRepository

    {
        private readonly AppDbContext _context;

        public WasteReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WasteReport>> GetAllAsync()
        {
            return await _context.WasteReports
                .Include(w => w.Citizen)
                .Include(w => w.Area)
                .ToListAsync();
        }

        public async Task<WasteReport?> GetByIdAsync(int id)
        {
            return await _context.WasteReports
                .Include(w => w.Citizen)
                .Include(w => w.Area)
                .FirstOrDefaultAsync(w => w.ReportId == id);
        }

        public async Task AddAsync(WasteReport wasteReport)
        {
            await _context.WasteReports.AddAsync(wasteReport);
        }

        public Task UpdateAsync(WasteReport wasteReport)
        {
            _context.WasteReports.Update(wasteReport);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(WasteReport wasteReport)
        {
            _context.WasteReports.Remove(wasteReport);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        

    }
}
