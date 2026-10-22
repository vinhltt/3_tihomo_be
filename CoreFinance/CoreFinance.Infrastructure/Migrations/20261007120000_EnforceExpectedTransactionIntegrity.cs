using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnforceExpectedTransactionIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // (EN) xmin is a PostgreSQL system column mapped as row version in the model only: no column is added.
            //      Index creation fails on pre-existing duplicates by design; data is never deduplicated here.
            // (VI) xmin là cột hệ thống PostgreSQL, chỉ ánh xạ row version trong model: không thêm cột.
            //      Tạo index thất bại khi dữ liệu đã trùng là có chủ đích; migration không bao giờ dedupe dữ liệu.
            migrationBuilder.DropIndex(
                name: "ix_expected_transactions_actual_transaction_id",
                table: "expected_transactions");

            migrationBuilder.CreateIndex(
                name: "ux_expected_actual_transaction",
                table: "expected_transactions",
                column: "actual_transaction_id",
                unique: true,
                filter: "actual_transaction_id IS NOT NULL");

            // (EN) One period per template per calendar day, regardless of status or soft delete.
            // (VI) Mỗi mẫu một kỳ cho mỗi ngày lịch, bất kể trạng thái hay xóa mềm.
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ux_expected_template_day ON expected_transactions " +
                "(recurring_transaction_template_id, ((expected_date)::date)) " +
                "WHERE recurring_transaction_template_id IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX ux_expected_template_day;");

            migrationBuilder.DropIndex(
                name: "ux_expected_actual_transaction",
                table: "expected_transactions");

            migrationBuilder.CreateIndex(
                name: "ix_expected_transactions_actual_transaction_id",
                table: "expected_transactions",
                column: "actual_transaction_id");
        }
    }
}
