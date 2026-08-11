using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    card_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    initial_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    current_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    available_limit = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    create_by = table.Column<string>(type: "text", nullable: true),
                    update_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recurring_transaction_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    transaction_type = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    frequency = table.Column<int>(type: "integer", nullable: false),
                    custom_interval_days = table.Column<int>(type: "integer", nullable: true),
                    next_execution_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cron_expression = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    auto_generate = table.Column<bool>(type: "boolean", nullable: false),
                    days_in_advance = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_by = table.Column<string>(type: "text", nullable: true),
                    update_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recurring_transaction_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_recurring_transaction_templates_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    transaction_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    revenue_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    spent_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    balance_compare = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    available_limit = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    available_limit_compare = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    transaction_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    sync_misa = table.Column<bool>(type: "boolean", nullable: false),
                    sync_sms = table.Column<bool>(type: "boolean", nullable: false),
                    vn = table.Column<bool>(type: "boolean", nullable: false),
                    category_summary = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    import_from = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    increase_credit_limit = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    used_percent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    category_type = table.Column<int>(type: "integer", nullable: false),
                    group = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_by = table.Column<string>(type: "text", nullable: true),
                    update_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_transactions_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "expected_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recurring_transaction_template_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    actual_transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expected_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expected_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    transaction_type = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_adjusted = table.Column<bool>(type: "boolean", nullable: false),
                    original_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    adjustment_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    generated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    create_by = table.Column<string>(type: "text", nullable: true),
                    update_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expected_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_expected_transactions_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_expected_transactions_recurring_transaction_templates_recur",
                        column: x => x.recurring_transaction_template_id,
                        principalTable: "recurring_transaction_templates",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_expected_transactions_transactions_actual_transaction_id",
                        column: x => x.actual_transaction_id,
                        principalTable: "transactions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_accounts_id",
                table: "accounts",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_expected_transactions_account_id",
                table: "expected_transactions",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_expected_transactions_actual_transaction_id",
                table: "expected_transactions",
                column: "actual_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_expected_transactions_id",
                table: "expected_transactions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_expected_transactions_recurring_transaction_template_id",
                table: "expected_transactions",
                column: "recurring_transaction_template_id");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_transaction_templates_account_id",
                table: "recurring_transaction_templates",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_transaction_templates_id",
                table: "recurring_transaction_templates",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_account_id",
                table: "transactions",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_id",
                table: "transactions",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expected_transactions");

            migrationBuilder.DropTable(
                name: "recurring_transaction_templates");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "accounts");
        }
    }
}
