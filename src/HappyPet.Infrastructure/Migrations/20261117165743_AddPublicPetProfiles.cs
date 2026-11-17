using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyPet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicPetProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublicProfileEnabled",
                table: "Pets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PublicProfileToken",
                table: "Pets",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pets_PublicProfileToken",
                table: "Pets",
                column: "PublicProfileToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pets_PublicProfileToken",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "IsPublicProfileEnabled",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "PublicProfileToken",
                table: "Pets");
        }
    }
}
