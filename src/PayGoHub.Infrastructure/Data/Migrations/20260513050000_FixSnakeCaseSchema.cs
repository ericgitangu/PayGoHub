using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayGoHub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixSnakeCaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename quoted PascalCase AccountNumber to snake_case (Npgsql 10 convention)
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

            // Drop ActivityLogs (PascalCase) if it exists — recreate as activity_logs
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS ""ActivityLogs"" CASCADE;
            ");

            // Create activity_logs with snake_case columns matching Npgsql 10 runtime model
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS activity_logs (
                    id UUID NOT NULL PRIMARY KEY,
                    activity_type TEXT NOT NULL,
                    title TEXT NOT NULL,
                    description TEXT NOT NULL,
                    entity_type TEXT,
                    entity_id UUID,
                    entity_identifier TEXT,
                    status TEXT NOT NULL,
                    performed_by TEXT,
                    icon_class TEXT NOT NULL,
                    color_class TEXT NOT NULL,
                    metadata TEXT,
                    created_at TIMESTAMPTZ NOT NULL,
                    updated_at TIMESTAMPTZ NOT NULL,
                    deleted_at TIMESTAMPTZ
                );
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_activity_logs_entity_type"" ON activity_logs (entity_type);
                CREATE INDEX IF NOT EXISTS ""IX_activity_logs_entity_id"" ON activity_logs (entity_id);
                CREATE INDEX IF NOT EXISTS ""IX_activity_logs_entity_identifier"" ON activity_logs (entity_identifier);
                CREATE INDEX IF NOT EXISTS ""IX_activity_logs_created_at"" ON activity_logs (created_at);
            ");

            // Ensure country and currency columns exist on customers (from AddMServicesEntities raw SQL)
            migrationBuilder.Sql(@"
                ALTER TABLE customers ADD COLUMN IF NOT EXISTS account_number TEXT;
                ALTER TABLE customers ADD COLUMN IF NOT EXISTS country VARCHAR(10) DEFAULT 'KE';
                ALTER TABLE customers ADD COLUMN IF NOT EXISTS currency VARCHAR(10) DEFAULT 'KES';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS activity_logs;");
            migrationBuilder.Sql(@"ALTER TABLE customers RENAME COLUMN account_number TO ""AccountNumber"";");
        }
    }
}
