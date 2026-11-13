using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HappyPet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketplace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketplaceListings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SellerId = table.Column<int>(type: "integer", nullable: false),
                    PetId = table.Column<int>(type: "integer", nullable: true),
                    PetName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Species = table.Column<int>(type: "integer", nullable: false),
                    Breed = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Size = table.Column<int>(type: "integer", nullable: true),
                    IsSterilized = table.Column<bool>(type: "boolean", nullable: false),
                    IsVaccinated = table.Column<bool>(type: "boolean", nullable: false),
                    MicrochipNumber = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    ListingType = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<int>(type: "integer", nullable: true),
                    BlockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BlockedByUserId = table.Column<int>(type: "integer", nullable: true),
                    BlockReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WithdrawnAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketplaceListings_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MarketplaceListings_Users_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceListingPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ListingId = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceListingPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketplaceListingPhotos_MarketplaceListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "MarketplaceListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OwnershipTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ListingId = table.Column<int>(type: "integer", nullable: false),
                    PetId = table.Column<int>(type: "integer", nullable: true),
                    MicrochipNumber = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    SellerId = table.Column<int>(type: "integer", nullable: false),
                    BuyerUserId = table.Column<int>(type: "integer", nullable: true),
                    BuyerFullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BuyerPersonalId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    BuyerPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TransferType = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TransferredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AirsSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AirsReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnershipTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OwnershipTransfers_MarketplaceListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "MarketplaceListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceListingPhotos_ListingId",
                table: "MarketplaceListingPhotos",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceListings_City",
                table: "MarketplaceListings",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceListings_PetId",
                table: "MarketplaceListings",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceListings_SellerId",
                table: "MarketplaceListings",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceListings_Species_ListingType",
                table: "MarketplaceListings",
                columns: new[] { "Species", "ListingType" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceListings_Status_ExpiresAt",
                table: "MarketplaceListings",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OwnershipTransfers_ListingId",
                table: "OwnershipTransfers",
                column: "ListingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OwnershipTransfers_Status_TransferredAt",
                table: "OwnershipTransfers",
                columns: new[] { "Status", "TransferredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketplaceListingPhotos");

            migrationBuilder.DropTable(
                name: "OwnershipTransfers");

            migrationBuilder.DropTable(
                name: "MarketplaceListings");
        }
    }
}
