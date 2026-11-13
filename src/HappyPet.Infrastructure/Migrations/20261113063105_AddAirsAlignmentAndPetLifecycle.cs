using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyPet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAirsAlignmentAndPetLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImmunityYears",
                table: "Vaccinations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNextDueOverridden",
                table: "Vaccinations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerCountry",
                table: "Vaccinations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VaccinationType",
                table: "Vaccinations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeathCause",
                table: "Pets",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeceasedAt",
                table: "Pets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeceased",
                table: "Pets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSterilized",
                table: "Pets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Size",
                table: "Pets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SterilizedAt",
                table: "Pets",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImmunityYears",
                table: "Vaccinations");

            migrationBuilder.DropColumn(
                name: "IsNextDueOverridden",
                table: "Vaccinations");

            migrationBuilder.DropColumn(
                name: "ManufacturerCountry",
                table: "Vaccinations");

            migrationBuilder.DropColumn(
                name: "VaccinationType",
                table: "Vaccinations");

            migrationBuilder.DropColumn(
                name: "DeathCause",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "DeceasedAt",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "IsDeceased",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "IsSterilized",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "SterilizedAt",
                table: "Pets");
        }
    }
}
