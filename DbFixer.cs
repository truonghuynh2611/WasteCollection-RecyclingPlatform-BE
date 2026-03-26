using System;
using Npgsql;

class Program
{
    static void Main()
    {
        string connString = "Host=localhost;Port=5432;Database=waste_management;Username=postgres;Password=123";
        try
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                Console.WriteLine("Connected to DB.");
                
                using (var cmd = new NpgsqlCommand(@"ALTER TABLE ""ReportImages"" ADD COLUMN IF NOT EXISTS ""ImageType"" VARCHAR(50);", conn))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Added ImageType column to ReportImages (if not existed).");
                }
                
                using (var cmd = new NpgsqlCommand(@"ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""TokenVersion"" INTEGER NOT NULL DEFAULT 0;", conn))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Added TokenVersion column to Users (if not existed).");
                }
            }
            Console.WriteLine("Schema fix completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fixing schema: {ex.Message}");
        }
    }
}
