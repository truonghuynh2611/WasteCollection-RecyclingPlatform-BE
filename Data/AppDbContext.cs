using Microsoft.EntityFrameworkCore;
using WasteReportApp.Models.Entities;

namespace WasteReportApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Citizen> Citizens { get; set; }
        public DbSet<WasteReport> WasteReports { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<District> Districts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Citizen>()
                .HasKey(c => c.CitizenId);

            modelBuilder.Entity<WasteReport>()
                .HasKey(w => w.ReportId);

            modelBuilder.Entity<WasteReport>()
                .HasOne(w => w.Citizen)
                .WithMany(c => c.WasteReports)
                .HasForeignKey(w => w.CitizenId);

            modelBuilder.Entity<WasteReport>()
                .HasOne(w => w.Area)
                .WithMany(a => a.WasteReports)
                .HasForeignKey(w => w.AreaId);

            modelBuilder.Entity<Area>()
                .HasKey(a => a.AreaId);

            modelBuilder.Entity<Area>()
                .HasOne(a => a.District)
                .WithMany(d => d.Areas)
                .HasForeignKey(a => a.DistrictId);

            modelBuilder.Entity<District>()
                .HasKey(d => d.DistrictId);
        }
    }

}
