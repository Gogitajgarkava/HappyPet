using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyPet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnershipTransferProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuyerAddress",
                table: "OwnershipTransfers",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatorNotes",
                table: "OwnershipTransfers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "OwnershipTransfers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProcessedByUserId",
                table: "OwnershipTransfers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "OwnershipTransfers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OwnershipTransfers_Status_ProcessedAt",
                table: "OwnershipTransfers",
                columns: new[] { "Status", "ProcessedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OwnershipTransfers_Status_ProcessedAt",
                table: "OwnershipTransfers");

            migrationBuilder.DropColumn(
                name: "BuyerAddress",
                table: "OwnershipTransfers");

            migrationBuilder.DropColumn(
                name: "OperatorNotes",
                table: "OwnershipTransfers");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "OwnershipTransfers");

            migrationBuilder.DropColumn(
                name: "ProcessedByUserId",
                table: "OwnershipTransfers");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "OwnershipTransfers");
        }
    }
}
