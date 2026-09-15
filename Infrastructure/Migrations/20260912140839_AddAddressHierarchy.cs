using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoBridge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "Donations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressDetails",
                table: "Accounts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Donations_CityId",
                table: "Donations",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_Cities_CityId",
                table: "Donations",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Donations_Cities_CityId",
                table: "Donations");

            migrationBuilder.DropIndex(
                name: "IX_Donations_CityId",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "AddressDetails",
                table: "Accounts");
        }
    }
}
