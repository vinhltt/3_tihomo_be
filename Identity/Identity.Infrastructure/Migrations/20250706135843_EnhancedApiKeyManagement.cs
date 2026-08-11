using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedApiKeyManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "usage_count",
                table: "api_keys",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "daily_usage_quota",
                table: "api_keys",
                type: "integer",
                nullable: false,
                defaultValue: 10000);

            migrationBuilder.AddColumn<string>(
                name: "ip_whitelist",
                table: "api_keys",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_reset_date",
                table: "api_keys",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "rate_limit_per_minute",
                table: "api_keys",
                type: "integer",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddColumn<DateTime>(
                name: "revoked_at",
                table: "api_keys",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "security_settings",
                table: "api_keys",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "today_usage_count",
                table: "api_keys",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "api_key_usage_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    api_key_id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    method = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    endpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status_code = table.Column<int>(type: "integer", nullable: false),
                    response_time = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    user_agent = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    request_size = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    response_size = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    request_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    scopes_used = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    create_by = table.Column<string>(type: "text", nullable: true),
                    update_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_api_key_usage_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_api_key_usage_logs_api_keys_api_key_id",
                        column: x => x.api_key_id,
                        principalTable: "api_keys",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_api_keys_expires_at",
                table: "api_keys",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_api_keys_status",
                table: "api_keys",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_api_key_usage_logs_api_key_id",
                table: "api_key_usage_logs",
                column: "api_key_id");

            migrationBuilder.CreateIndex(
                name: "ix_api_key_usage_logs_api_key_id_timestamp",
                table: "api_key_usage_logs",
                columns: new[] { "api_key_id", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_api_key_usage_logs_id",
                table: "api_key_usage_logs",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_api_key_usage_logs_ip_address",
                table: "api_key_usage_logs",
                column: "ip_address");

            migrationBuilder.CreateIndex(
                name: "ix_api_key_usage_logs_method",
                table: "api_key_usage_logs",
                column: "method");

            migrationBuilder.CreateIndex(
                name: "ix_api_key_usage_logs_status_code",
                table: "api_key_usage_logs",
                column: "status_code");

            migrationBuilder.CreateIndex(
                name: "ix_api_key_usage_logs_timestamp",
                table: "api_key_usage_logs",
                column: "timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_key_usage_logs");

            migrationBuilder.DropIndex(
                name: "ix_api_keys_expires_at",
                table: "api_keys");

            migrationBuilder.DropIndex(
                name: "ix_api_keys_status",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "daily_usage_quota",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "ip_whitelist",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "last_reset_date",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "rate_limit_per_minute",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "revoked_at",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "security_settings",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "today_usage_count",
                table: "api_keys");

            migrationBuilder.AlterColumn<int>(
                name: "usage_count",
                table: "api_keys",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 0L);
        }
    }
}
