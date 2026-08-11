using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNoAndLastSyncTransactionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "no",
                table: "transactions",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "last_sync_transaction_id",
                table: "accounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "no",
                table: "accounts",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "no",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "last_sync_transaction_id",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "no",
                table: "accounts");
        }
    }
}
