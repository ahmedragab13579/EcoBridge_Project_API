using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoBridge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAccountIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Accounts",
                column: "NormalizedUserName",
                unique: true,
                filter: "[IsDeleted]=0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Accounts",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }
    }
}
