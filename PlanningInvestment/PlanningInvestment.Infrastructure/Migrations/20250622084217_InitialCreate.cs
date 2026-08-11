using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanningInvestment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "debts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    debt_type = table.Column<int>(type: "integer", nullable: false),
                    original_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    current_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    interest_rate = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    minimum_payment = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    creditor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    account_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    target_payoff_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    create_by = table.Column<string>(type: "text", nullable: true),
                    update_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_debts", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_debts_id",
                table: "debts",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "debts");
        }
    }
}
