using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayGoHub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMServicesEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add columns using conditional SQL to handle if they already exist
            migrationBuilder.Sql("ALTER TABLE payments ADD COLUMN IF NOT EXISTS provider_key VARCHAR(100);");
            migrationBuilder.Sql("ALTER TABLE loans ADD COLUMN IF NOT EXISTS \"Currency\" TEXT DEFAULT '';");
            migrationBuilder.Sql("ALTER TABLE devices ADD COLUMN IF NOT EXISTS \"Country\" TEXT DEFAULT '';");
            migrationBuilder.Sql("ALTER TABLE devices ADD COLUMN IF NOT EXISTS \"Type\" INTEGER DEFAULT 0;");
            migrationBuilder.Sql("ALTER TABLE customers ADD COLUMN IF NOT EXISTS \"AccountNumber\" TEXT;");
            migrationBuilder.Sql("ALTER TABLE customers ADD COLUMN IF NOT EXISTS country VARCHAR(10) DEFAULT 'KE';");
            migrationBuilder.Sql("ALTER TABLE customers ADD COLUMN IF NOT EXISTS currency VARCHAR(10) DEFAULT 'KES';");

            // Create tables using raw SQL with IF NOT EXISTS
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS api_clients (
                    id UUID PRIMARY KEY,
                    name VARCHAR(200) NOT NULL,
                    api_key_hash VARCHAR(64) NOT NULL,
                    is_active BOOLEAN DEFAULT TRUE,
                    allowed_scopes TEXT[] NOT NULL DEFAULT '{}',
                    allowed_providers TEXT[] NOT NULL DEFAULT '{}',
                    last_used_at TIMESTAMPTZ,
                    rate_limit_per_minute INTEGER DEFAULT 100,
                    ip_whitelist VARCHAR(1000),
                    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                    deleted_at TIMESTAMPTZ
                );
                CREATE UNIQUE INDEX IF NOT EXISTS IX_api_clients_api_key_hash ON api_clients(api_key_hash);
                CREATE INDEX IF NOT EXISTS IX_api_clients_name ON api_clients(name);
            ");

            migrationBuilder.CreateTable(
                name: "momo_payment_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    amount_subunit = table.Column<long>(type: "bigint", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "KES"),
                    business_account = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    provider_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sender_phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    provider_tx = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    momoep_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    error_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    validated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    secondary_provider_tx = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    transaction_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sender_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    transaction_kind = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_momo_payment_transactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "providers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    country = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "KES"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    configuration_json = table.Column<string>(type: "jsonb", nullable: true),
                    min_amount_subunit = table.Column<long>(type: "bigint", nullable: false, defaultValue: 100L),
                    max_amount_subunit = table.Column<long>(type: "bigint", nullable: false, defaultValue: 100000000L),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_providers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "device_commands",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_identifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    identifier_kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "serial"),
                    command_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    command_details = table.Column<string>(type: "jsonb", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    callback_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    device_response = table.Column<string>(type: "jsonb", nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    executed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    callback_delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    api_client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_commands", x => x.id);
                    table.ForeignKey(
                        name: "FK_device_commands_api_clients_api_client_id",
                        column: x => x.api_client_id,
                        principalTable: "api_clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_identifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    token_value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    command = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "unlock_relative"),
                    payload = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sequence_number = table.Column<int>(type: "integer", nullable: false),
                    encoding = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    days_credit = table.Column<int>(type: "integer", nullable: true),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    api_client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_tokens_api_clients_api_client_id",
                        column: x => x.api_client_id,
                        principalTable: "api_clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_api_clients_api_key_hash",
                table: "api_clients",
                column: "api_key_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_api_clients_name",
                table: "api_clients",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_device_commands_api_client_id",
                table: "device_commands",
                column: "api_client_id");

            migrationBuilder.CreateIndex(
                name: "IX_device_commands_device_identifier_status",
                table: "device_commands",
                columns: new[] { "device_identifier", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_device_commands_status",
                table: "device_commands",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_momo_payment_transactions_idempotency_key",
                table: "momo_payment_transactions",
                column: "idempotency_key",
                unique: true,
                filter: "idempotency_key IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_momo_payment_transactions_provider_tx_momoep_id",
                table: "momo_payment_transactions",
                columns: new[] { "provider_tx", "momoep_id" },
                unique: true,
                filter: "provider_tx IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_momo_payment_transactions_reference_provider_key",
                table: "momo_payment_transactions",
                columns: new[] { "reference", "provider_key" });

            migrationBuilder.CreateIndex(
                name: "IX_momo_payment_transactions_status",
                table: "momo_payment_transactions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_providers_country",
                table: "providers",
                column: "country");

            migrationBuilder.CreateIndex(
                name: "IX_providers_provider_key",
                table: "providers",
                column: "provider_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tokens_api_client_id",
                table: "tokens",
                column: "api_client_id");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_device_identifier",
                table: "tokens",
                column: "device_identifier");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_device_identifier_sequence_number",
                table: "tokens",
                columns: new[] { "device_identifier", "sequence_number" });

            migrationBuilder.CreateIndex(
                name: "IX_tokens_token_value",
                table: "tokens",
                column: "token_value",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device_commands");

            migrationBuilder.DropTable(
                name: "momo_payment_transactions");

            migrationBuilder.DropTable(
                name: "providers");

            migrationBuilder.DropTable(
                name: "tokens");

            migrationBuilder.DropTable(
                name: "api_clients");

            migrationBuilder.DropColumn(
                name: "provider_key",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "loans");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "country",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "customers");
        }
    }
}
