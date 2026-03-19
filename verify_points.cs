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
    var logs = await context.PointHistories
        .OrderByDescending(p => p.CreatedAt)
        .Take(5)
        .ToListAsync();

    Console.WriteLine("LAST 5 POINT HISTORY RECORDS:");
    foreach (var log in logs)
    {
        Console.WriteLine($"ID: {log.PointlogId}, ReportId: {log.ReportId}, Points: {log.PointAmount}, CreatedAt: {log.CreatedAt}");
    }
}
