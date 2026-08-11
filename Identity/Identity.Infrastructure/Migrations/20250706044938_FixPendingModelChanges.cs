using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "o_auth_clients",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "post_logout_redirect_uris", "redirect_uris" },
                values: new object[] { "http://localhost:3500,https://app.tihomo.vn", "http://localhost:3500/auth/callback,https://app.tihomo.vn/auth/callback" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "o_auth_clients",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "post_logout_redirect_uris", "redirect_uris" },
                values: new object[] { "http://localhost:3333,https://app.tihomo.vn", "http://localhost:3333/auth/callback,https://app.tihomo.vn/auth/callback" });
        }
    }
}
