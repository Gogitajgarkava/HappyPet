using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HappyPet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVaccinations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Vaccinations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PetId = table.Column<int>(type: "integer", nullable: false),
                    VaccineName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Disease = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Manufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BatchNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CertificateNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VaccinatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextDueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VeterinarianName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ClinicName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AirsVaccinationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vaccinations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vaccinations_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vaccinations_PetId_AirsVaccinationId",
                table: "Vaccinations",
                columns: new[] { "PetId", "AirsVaccinationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vaccinations_PetId_VaccinatedAt",
                table: "Vaccinations",
                columns: new[] { "PetId", "VaccinatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vaccinations");
        }
    }
}
