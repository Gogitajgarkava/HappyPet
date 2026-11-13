using HappyPet.Domain.Enums;

namespace HappyPet.Application.Dtos.Requests
{
    public class CreateMarketplaceListingRequest
    {
        /// <summary>Optional link to an already registered pet.</summary>
        public int? PetId { get; set; }

        public string PetName { get; set; } = string.Empty;
        public PetSpecies Species { get; set; }
        public string? Breed { get; set; }
        public PetGender Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string? Color { get; set; }
        public PetSize? Size { get; set; }
        public bool IsSterilized { get; set; }
        public bool IsVaccinated { get; set; }
        public string? MicrochipNumber { get; set; }

        public ListingType ListingType { get; set; }
        public decimal? Price { get; set; }
        public string City { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateMarketplaceListingRequest
    {
        public int? PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public PetSpecies Species { get; set; }
        public string? Breed { get; set; }
        public PetGender Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string? Color { get; set; }
        public PetSize? Size { get; set; }
        public bool IsSterilized { get; set; }
        public bool IsVaccinated { get; set; }
        public string? MicrochipNumber { get; set; }

        public ListingType ListingType { get; set; }
        public decimal? Price { get; set; }
        public string City { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CompleteMarketplaceListingRequest
    {
        /// <summary>Optional registered buyer account.</summary>
        public int? BuyerUserId { get; set; }
        public string BuyerFullName { get; set; } = string.Empty;
        public string BuyerPersonalId { get; set; } = string.Empty;
        public string? BuyerPhone { get; set; }
        public DateTime? TransferredAt { get; set; }
        public string? Notes { get; set; }
    }

    public class BlockMarketplaceListingRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
