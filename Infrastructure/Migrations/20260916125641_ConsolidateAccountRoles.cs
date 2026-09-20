using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoBridge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateAccountRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryMissions_Volunteers_VolunteerId",
                table: "DeliveryMissions");

            migrationBuilder.DropForeignKey(
                name: "FK_DonationRequests_Charities_CharityId",
                table: "DonationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Donations_Charities_CharityId",
                table: "Donations");

            migrationBuilder.DropForeignKey(
                name: "FK_Donations_Donors_DonorId",
                table: "Donations");

            migrationBuilder.DropForeignKey(
                name: "FK_Donations_Volunteers_VolunteerId",
                table: "Donations");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Charities");

            migrationBuilder.DropTable(
                name: "Donors");

            migrationBuilder.DropTable(
                name: "Volunteers");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "OrganizationName",
                table: "Accounts",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "Accounts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleDetails",
                table: "Accounts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleType",
                table: "Accounts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryMissions_Accounts_VolunteerId",
                table: "DeliveryMissions",
                column: "VolunteerId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DonationRequests_Accounts_CharityId",
                table: "DonationRequests",
                column: "CharityId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_Accounts_CharityId",
                table: "Donations",
                column: "CharityId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_Accounts_DonorId",
                table: "Donations",
                column: "DonorId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_Accounts_VolunteerId",
                table: "Donations",
                column: "VolunteerId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryMissions_Accounts_VolunteerId",
                table: "DeliveryMissions");

            migrationBuilder.DropForeignKey(
                name: "FK_DonationRequests_Accounts_CharityId",
                table: "DonationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Donations_Accounts_CharityId",
                table: "Donations");

            migrationBuilder.DropForeignKey(
                name: "FK_Donations_Accounts_DonorId",
                table: "Donations");

            migrationBuilder.DropForeignKey(
                name: "FK_Donations_Accounts_VolunteerId",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "OrganizationName",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "VehicleDetails",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "Accounts");

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_Admins_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Charities",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OrganizationName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Charities", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_Charities_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Donors",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    OrganizationName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UserType = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donors", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_Donors_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Volunteers",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    VehicleDetails = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VehicleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Volunteers", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_Volunteers_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Charities_IsDeleted",
                table: "Charities",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Charities_RegistrationNumber",
                table: "Charities",
                column: "RegistrationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Volunteers_IsAvailable",
                table: "Volunteers",
                column: "IsAvailable");

            migrationBuilder.CreateIndex(
                name: "IX_Volunteers_IsDeleted",
                table: "Volunteers",
                column: "IsDeleted");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryMissions_Volunteers_VolunteerId",
                table: "DeliveryMissions",
                column: "VolunteerId",
                principalTable: "Volunteers",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DonationRequests_Charities_CharityId",
                table: "DonationRequests",
                column: "CharityId",
                principalTable: "Charities",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_Charities_CharityId",
                table: "Donations",
                column: "CharityId",
                principalTable: "Charities",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_Donors_DonorId",
                table: "Donations",
                column: "DonorId",
                principalTable: "Donors",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_Volunteers_VolunteerId",
                table: "Donations",
                column: "VolunteerId",
                principalTable: "Volunteers",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
