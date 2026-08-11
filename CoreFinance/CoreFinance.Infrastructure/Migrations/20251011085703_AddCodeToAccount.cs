using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "accounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_accounts_code",
                table: "accounts",
                column: "code",
                unique: true,
                filter: "code IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_accounts_code",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "code",
                table: "accounts");
        }
    }
}
