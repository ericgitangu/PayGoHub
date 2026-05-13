using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PayGoHub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDataProtectionKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ActivityLogs",
                table: "ActivityLogs");

            migrationBuilder.RenameTable(
                name: "ActivityLogs",
                newName: "activity_logs");

            migrationBuilder.RenameColumn(
                name: "AccountNumber",
                table: "customers",
                newName: "account_number");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "activity_logs",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "activity_logs",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Metadata",
                table: "activity_logs",
                newName: "metadata");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "activity_logs",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "activity_logs",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "activity_logs",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "PerformedBy",
                table: "activity_logs",
                newName: "performed_by");

            migrationBuilder.RenameColumn(
                name: "IconClass",
                table: "activity_logs",
                newName: "icon");

            migrationBuilder.RenameColumn(
                name: "EntityType",
                table: "activity_logs",
                newName: "entity_type");

            migrationBuilder.RenameColumn(
                name: "EntityIdentifier",
                table: "activity_logs",
                newName: "entity_identifier");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "activity_logs",
                newName: "entity_id");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "activity_logs",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "activity_logs",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ColorClass",
                table: "activity_logs",
                newName: "color_class");

            migrationBuilder.RenameColumn(
                name: "ActivityType",
                table: "activity_logs",
                newName: "action_type");

            migrationBuilder.RenameIndex(
                name: "IX_ActivityLogs_EntityType",
                table: "activity_logs",
                newName: "IX_activity_logs_entity_type");

            migrationBuilder.RenameIndex(
                name: "IX_ActivityLogs_EntityIdentifier",
                table: "activity_logs",
                newName: "IX_activity_logs_entity_identifier");

            migrationBuilder.RenameIndex(
                name: "IX_ActivityLogs_EntityId",
                table: "activity_logs",
                newName: "IX_activity_logs_entity_id");

            migrationBuilder.RenameIndex(
                name: "IX_ActivityLogs_CreatedAt",
                table: "activity_logs",
                newName: "IX_activity_logs_created_at");

            migrationBuilder.AlterColumn<string>(
                name: "account_number",
                table: "customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "activity_logs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "activity_logs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "metadata",
                table: "activity_logs",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "activity_logs",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "performed_by",
                table: "activity_logs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "icon",
                table: "activity_logs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "entity_type",
                table: "activity_logs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "entity_identifier",
                table: "activity_logs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "color_class",
                table: "activity_logs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "action_type",
                table: "activity_logs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_activity_logs",
                table: "activity_logs",
                column: "id");

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });
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
