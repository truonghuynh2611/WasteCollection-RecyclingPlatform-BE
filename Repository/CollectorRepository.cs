using Microsoft.EntityFrameworkCore;
using WasteReportApp.Data;
using WasteReportApp.Models.Entities;

namespace WasteReportApp.Repository
{
    public class CollectorRepository : ICollectorRepository
    {
        private readonly AppDbContext _context;

        public CollectorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Collector?> GetByIdAsync(int id)
        {
            return await _context.Collectors
                .Include(c => c.Team)
                .FirstOrDefaultAsync(c => c.CollectorId == id);
        }

        public Task UpdateAsync(Collector collector)
        {
            _context.Collectors.Update(collector);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
