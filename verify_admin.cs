using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.DataAccess.Context;
using WasteCollectionPlatform.DataAccess.Entities;

var optionsBuilder = new DbContextOptionsBuilder<WasteManagementContext>();
optionsBuilder.UseNpgsql("Host=localhost;Database=WasteManagementDB;Username=postgres;Password=postgres");

using var context = new WasteManagementContext(optionsBuilder.Options);

var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@example.com");
if (user != null)
{
    Console.WriteLine($"UserId: {user.UserId}");
    Console.WriteLine($"Email: {user.Email}");
    Console.WriteLine($"Role: {user.Role}");
    Console.WriteLine($"Status: {user.Status}");
}
else
{
    Console.WriteLine("User not found: admin@example.com");
}

var allAdmins = await context.Users.Where(u => u.Role == WasteCollectionPlatform.Common.Enums.UserRole.Admin).ToListAsync();
Console.WriteLine("\nAll Admins in DB:");
foreach (var admin in allAdmins)
{
    Console.WriteLine($"- UserId: {admin.UserId}, Email: {admin.Email}, Status: {admin.Status}");
}
