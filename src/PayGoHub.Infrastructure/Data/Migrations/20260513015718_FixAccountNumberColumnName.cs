using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayGoHub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixAccountNumberColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename quoted PascalCase "AccountNumber" → snake_case account_number to match
            // HasColumnName("account_number") in CustomerConfiguration.
            // Guard handles both fresh DBs (already renamed by earlier migration) and existing
            // DBs where prior migration failed before completing the rename.
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'customers' AND column_name = 'AccountNumber'
                    ) THEN
                        ALTER TABLE customers RENAME COLUMN ""AccountNumber"" TO account_number;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'customers' AND column_name = 'account_number'
                    ) THEN
                        ALTER TABLE customers RENAME COLUMN account_number TO ""AccountNumber"";
                    END IF;
                END $$;
            ");
        }
    }
}
