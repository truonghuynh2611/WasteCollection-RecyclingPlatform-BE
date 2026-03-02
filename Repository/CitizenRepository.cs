using Microsoft.EntityFrameworkCore;
using WasteReportApp.Data;
using WasteReportApp.Models.Entities;

namespace WasteReportApp.Repository
{
    public class CitizenRepository : ICitizenRepository
    {
        private readonly AppDbContext _context;

        public CitizenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Citizen>> GetAllAsync()
        {
            return await _context.Citizens
                .Include(c => c.WasteReports)
                .ToListAsync();
        }

        public async Task<Citizen?> GetByIdAsync(int id)
        {
            return await _context.Citizens
                .Include(c => c.WasteReports)
                .FirstOrDefaultAsync(c => c.CitizenId == id);
        }

        public async Task AddAsync(Citizen citizen)
        {
            await _context.Citizens.AddAsync(citizen);
        }

        public Task UpdateAsync(Citizen citizen)
        {
            _context.Citizens.Update(citizen);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Citizen citizen)
        {
            _context.Citizens.Remove(citizen);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Citizens.AnyAsync(c => c.CitizenId == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
