using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyPet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLostPetTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Pets",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FoundAt",
                table: "Pets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSeenAddress",
                table: "Pets",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenAt",
                table: "Pets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastSeenLatitude",
                table: "Pets",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastSeenLongitude",
                table: "Pets",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LostAt",
                table: "Pets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pets_IsLost_LostAt",
                table: "Pets",
                columns: new[] { "IsLost", "LostAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pets_IsLost_LostAt",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "FoundAt",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "LastSeenAddress",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "LastSeenLatitude",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "LastSeenLongitude",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "LostAt",
                table: "Pets");
        }
    }
}
