using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoBridge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTrustScoreConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrustCreditLogs_Accounts_AccountId1",
                table: "TrustCreditLogs");

            migrationBuilder.DropIndex(
                name: "IX_TrustCreditLogs_AccountId1",
                table: "TrustCreditLogs");

            migrationBuilder.DropColumn(
                name: "AccountId1",
                table: "TrustCreditLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AccountId1",
                table: "TrustCreditLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrustCreditLogs_AccountId1",
                table: "TrustCreditLogs",
                column: "AccountId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TrustCreditLogs_Accounts_AccountId1",
                table: "TrustCreditLogs",
                column: "AccountId1",
                principalTable: "Accounts",
                principalColumn: "Id");
        }
    }
}
