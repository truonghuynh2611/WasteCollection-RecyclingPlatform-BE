using System;
using Npgsql;

var connString = "Host=localhost;Port=5432;Database=waste_management;Username=postgres;Password=123456";
try
{
    using var conn = new NpgsqlConnection(connString);
    conn.Open();
    using var cmd = new NpgsqlCommand(@"
        SELECT column_name, column_default, is_identity 
        FROM information_schema.columns 
        WHERE table_name = 'Areas' AND column_name = 'AreaId'", conn);
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        Console.WriteLine($"COLUMN: {reader[0]}, DEFAULT: {reader[1]}, IDENTITY: {reader[2]}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.Message}");
}
