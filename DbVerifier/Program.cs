using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false);

var configuration = builder.Build();
var connectionString = configuration.GetConnectionString("DefaultConnection");

var services = new ServiceCollection();
services.AddDbContext<WasteManagementContext>(options =>
    options.UseNpgsql(connectionString));

var serviceProvider = services.BuildServiceProvider();

using (var scope = serviceProvider.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WasteManagementContext>();
    Console.WriteLine("Fixing DB schema...");
    try {
        await context.Database.ExecuteSqlRawAsync(@"ALTER TABLE ""ReportImages"" ADD COLUMN IF NOT EXISTS ""ImageType"" VARCHAR(50);");
        Console.WriteLine("✅ ReportImages.ImageType fixed.");
    } catch (Exception ex) { Console.WriteLine($"❌ ReportImages fix error: {ex.Message}"); }

    try {
        await context.Database.ExecuteSqlRawAsync(@"ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""TokenVersion"" INTEGER NOT NULL DEFAULT 0;");
        Console.WriteLine("✅ Users.TokenVersion fixed.");
    } catch (Exception ex) { Console.WriteLine($"❌ Users fix error: {ex.Message}"); }
    
    Console.WriteLine("DB schema fix finished.");
}
