using Npgsql;

var connectionString = "Host=localhost;Port=5432;Database=waste_management;Username=postgres;Password=123456";

try
{
    using var conn = new NpgsqlConnection(connectionString);
    conn.Open();
    Console.WriteLine("✅ Connected to PostgreSQL");

    var sql = @"
-- Create refreshtoken table
CREATE TABLE IF NOT EXISTS refreshtoken (
    refreshtokenid SERIAL PRIMARY KEY,
    userid INTEGER NOT NULL,
    token VARCHAR(500) NOT NULL UNIQUE,
    expiresat TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    createdat TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    isrevoked BOOLEAN DEFAULT FALSE,
    revokedat TIMESTAMP WITHOUT TIME ZONE,
    CONSTRAINT fk_refreshtoken_user FOREIGN KEY (userid) REFERENCES ""User""(userid) ON DELETE CASCADE
);

-- Create indexes for performance
CREATE UNIQUE INDEX IF NOT EXISTS idx_refreshtoken_token ON refreshtoken(token);
CREATE INDEX IF NOT EXISTS idx_refreshtoken_userid ON refreshtoken(userid);
";

    using var cmd = new NpgsqlCommand(sql, conn);
    cmd.ExecuteNonQuery();
    
    Console.WriteLine("✅ RefreshToken table created successfully!");
    Console.WriteLine("✅ Indexes created successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    return 1;
}

return 0;
