using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WasteCollectionPlatform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
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
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS idx_refreshtoken_userid;
                DROP INDEX IF EXISTS idx_refreshtoken_token;
                DROP TABLE IF EXISTS refreshtoken;
            ");
        }
    }
}
