using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;

var optionsBuilder = new DbContextOptionsBuilder<WasteManagementContext>();
optionsBuilder.UseNpgsql("Host=localhost;Database=WasteManagementDB;Username=postgres;Password=postgres");

using var context = new WasteManagementContext(optionsBuilder.Options);

var admins = await context.Users.Where(u => u.Role == WasteCollectionPlatform.Common.Enums.UserRole.Admin).ToListAsync();
Console.WriteLine($"Found {admins.Count} admins:");
foreach (var admin in admins)
{
    Console.WriteLine($"- UserId: {admin.UserId}, Email: {admin.Email}, Role: {admin.Role}, Status: {admin.Status}");
}
