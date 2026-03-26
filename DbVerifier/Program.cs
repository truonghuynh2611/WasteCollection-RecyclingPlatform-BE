using System;
using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Npgsql;

var builder = Host.CreateApplicationBuilder(args);
var connectionString = "Host=localhost;Database=WasteManagement;Username=postgres;Password=postgres"; // Adjust if needed
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
var dataSource = dataSourceBuilder.Build();

var options = new DbContextOptionsBuilder<WasteManagementContext>()
    .UseNpgsql(dataSource)
    .Options;

using var context = new WasteManagementContext(options);

try {
    Console.WriteLine("--- Checking PendingRegistrations Columns ---");
    var pendingCols = context.Database.SqlQueryRaw<string>(@"
        SELECT column_name 
        FROM information_schema.columns 
        WHERE table_name = 'PendingRegistrations'
    ").ToList();
    foreach (var col in pendingCols) Console.WriteLine($"- {col}");

    Console.WriteLine("\n--- Checking WasteReports Columns ---");
    var reportCols = context.Database.SqlQueryRaw<string>(@"
        SELECT column_name 
        FROM information_schema.columns 
        WHERE table_name = 'WasteReports'
    ").ToList();
    foreach (var col in reportCols) Console.WriteLine($"- {col}");

    Console.WriteLine("\n--- Checking WasteReportItems Table ---");
    var hasItemsTable = context.Database.SqlQueryRaw<bool>(@"
        SELECT EXISTS (
            SELECT FROM information_schema.tables 
            WHERE table_name = 'WasteReportItems'
        )
    ").ToList().FirstOrDefault();
    Console.WriteLine($"WasteReportItems exists: {hasItemsTable}");

} catch (Exception ex) {
    Console.WriteLine($"Error: {ex.Message}");
}
