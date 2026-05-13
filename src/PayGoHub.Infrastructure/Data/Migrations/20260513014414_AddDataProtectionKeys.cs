using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayGoHub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDataProtectionKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename operations were already applied by FixSnakeCaseSchema (20260513050000).
            // This migration only creates the DataProtectionKeys table needed by ASP.NET
            // Data Protection for key ring persistence across Cloud Run container restarts.
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""DataProtectionKeys"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""FriendlyName"" TEXT,
                    ""Xml"" TEXT
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_activity_logs",
                table: "activity_logs");

            migrationBuilder.RenameTable(
                name: "activity_logs",
                newName: "ActivityLogs");

            migrationBuilder.RenameColumn(
                name: "account_number",
                table: "customers",
                newName: "AccountNumber");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "ActivityLogs",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "ActivityLogs",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "metadata",
                table: "ActivityLogs",
                newName: "Metadata");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "ActivityLogs",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ActivityLogs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "ActivityLogs",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "performed_by",
                table: "ActivityLogs",
                newName: "PerformedBy");

            migrationBuilder.RenameColumn(
                name: "icon",
                table: "ActivityLogs",
                newName: "IconClass");

            migrationBuilder.RenameColumn(
                name: "entity_type",
                table: "ActivityLogs",
                newName: "EntityType");

            migrationBuilder.RenameColumn(
                name: "entity_identifier",
                table: "ActivityLogs",
                newName: "EntityIdentifier");

            migrationBuilder.RenameColumn(
                name: "entity_id",
                table: "ActivityLogs",
                newName: "EntityId");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "ActivityLogs",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ActivityLogs",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "color_class",
                table: "ActivityLogs",
                newName: "ColorClass");

            migrationBuilder.RenameColumn(
                name: "action_type",
                table: "ActivityLogs",
                newName: "ActivityType");

            migrationBuilder.RenameIndex(
                name: "IX_activity_logs_entity_type",
                table: "ActivityLogs",
                newName: "IX_ActivityLogs_EntityType");

            migrationBuilder.RenameIndex(
                name: "IX_activity_logs_entity_identifier",
                table: "ActivityLogs",
                newName: "IX_ActivityLogs_EntityIdentifier");

            migrationBuilder.RenameIndex(
                name: "IX_activity_logs_entity_id",
                table: "ActivityLogs",
                newName: "IX_ActivityLogs_EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_activity_logs_created_at",
                table: "ActivityLogs",
                newName: "IX_ActivityLogs_CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "customers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ActivityLogs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ActivityLogs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Metadata",
                table: "ActivityLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ActivityLogs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "PerformedBy",
                table: "ActivityLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IconClass",
                table: "ActivityLogs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                table: "ActivityLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EntityIdentifier",
                table: "ActivityLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ColorClass",
                table: "ActivityLogs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ActivityType",
                table: "ActivityLogs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActivityLogs",
                table: "ActivityLogs",
                column: "Id");
        }
    }
}
