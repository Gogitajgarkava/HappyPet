using HappyPet.Domain.Enums;

namespace HappyPet.Application.Dtos.Responses
{
    public class MarketplaceListingResponse
    {
        public int Id { get; set; }
        public int SellerId { get; set; }
        public int? PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public PetSpecies Species { get; set; }
        public string? Breed { get; set; }
        public PetGender Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public int AgeWeeks { get; set; }
        public string? Color { get; set; }
        public PetSize? Size { get; set; }
        public bool IsSterilized { get; set; }
        public bool IsVaccinated { get; set; }
        public string? MicrochipNumber { get; set; }
        public ListingType ListingType { get; set; }
        public decimal? Price { get; set; }
        public string Currency { get; set; } = "GEL";
        public string City { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ListingStatus Status { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? BlockedAt { get; set; }
        public string? BlockReason { get; set; }
        public DateTime? WithdrawnAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MarketplaceListingPhotoResponse> Photos { get; set; } = new();
    }

    public class MarketplaceListingPhotoResponse
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OwnershipTransferResponse
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public int? PetId { get; set; }
        public string? MicrochipNumber { get; set; }
        public int SellerId { get; set; }
        public int? BuyerUserId { get; set; }
        public string BuyerFullName { get; set; } = string.Empty;
        public string BuyerPersonalId { get; set; } = string.Empty;
        public string? BuyerPhone { get; set; }
        public OwnershipTransferType TransferType { get; set; }
        public decimal? Price { get; set; }
        public string Currency { get; set; } = "GEL";
        public OwnershipTransferStatus Status { get; set; }
        public DateTime TransferredAt { get; set; }
        public DateTime? AirsSyncedAt { get; set; }
        public string? AirsReference { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CompleteMarketplaceListingResponse
    {
        public MarketplaceListingResponse Listing { get; set; } = null!;
        public OwnershipTransferResponse OwnershipTransfer { get; set; } = null!;
    }
}
