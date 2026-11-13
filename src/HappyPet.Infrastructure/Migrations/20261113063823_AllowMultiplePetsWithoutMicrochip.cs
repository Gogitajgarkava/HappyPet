using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyPet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultiplePetsWithoutMicrochip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pets_MicrochipNumber",
                table: "Pets");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_MicrochipNumber",
                table: "Pets",
                column: "MicrochipNumber",
                unique: true,
                filter: "\"MicrochipNumber\" <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pets_MicrochipNumber",
                table: "Pets");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_MicrochipNumber",
                table: "Pets",
                column: "MicrochipNumber",
                unique: true);
        }
    }
}
