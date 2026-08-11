using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanningInvestment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSnakeCaseNamingConvention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_debts",
                table: "debts");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "debts",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "debts",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "debts",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Creditor",
                table: "debts",
                newName: "creditor");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "debts",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "TargetPayoffDate",
                table: "debts",
                newName: "target_payoff_date");

            migrationBuilder.RenameColumn(
                name: "OriginalAmount",
                table: "debts",
                newName: "original_amount");

            migrationBuilder.RenameColumn(
                name: "MinimumPayment",
                table: "debts",
                newName: "minimum_payment");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "debts",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "InterestRate",
                table: "debts",
                newName: "interest_rate");

            migrationBuilder.RenameColumn(
                name: "DueDate",
                table: "debts",
                newName: "due_date");

            migrationBuilder.RenameColumn(
                name: "DebtType",
                table: "debts",
                newName: "debt_type");

            migrationBuilder.RenameColumn(
                name: "CurrentBalance",
                table: "debts",
                newName: "current_balance");

            migrationBuilder.RenameColumn(
                name: "AccountNumber",
                table: "debts",
                newName: "account_number");

            migrationBuilder.RenameIndex(
                name: "IX_debts_id",
                table: "debts",
                newName: "ix_debts_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_debts",
                table: "debts",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_debts",
                table: "debts");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "debts",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "debts",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "debts",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "creditor",
                table: "debts",
                newName: "Creditor");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "debts",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "target_payoff_date",
                table: "debts",
                newName: "TargetPayoffDate");

            migrationBuilder.RenameColumn(
                name: "original_amount",
                table: "debts",
                newName: "OriginalAmount");

            migrationBuilder.RenameColumn(
                name: "minimum_payment",
                table: "debts",
                newName: "MinimumPayment");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "debts",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "interest_rate",
                table: "debts",
                newName: "InterestRate");

            migrationBuilder.RenameColumn(
                name: "due_date",
                table: "debts",
                newName: "DueDate");

            migrationBuilder.RenameColumn(
                name: "debt_type",
                table: "debts",
                newName: "DebtType");

            migrationBuilder.RenameColumn(
                name: "current_balance",
                table: "debts",
                newName: "CurrentBalance");

            migrationBuilder.RenameColumn(
                name: "account_number",
                table: "debts",
                newName: "AccountNumber");

            migrationBuilder.RenameIndex(
                name: "ix_debts_id",
                table: "debts",
                newName: "IX_debts_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_debts",
                table: "debts",
                column: "id");
        }
    }
}
