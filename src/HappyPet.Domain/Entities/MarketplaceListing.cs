using HappyPet.Domain.Common;
using HappyPet.Domain.Enums;

namespace HappyPet.Domain.Entities
{
    /// <summary>
    /// A standalone marketplace listing. A listing may optionally reference a
    /// registered pet, but registration is not required (e.g. newborn puppies
    /// and kittens that do not have a microchip yet).
    /// </summary>
    public class MarketplaceListing : BaseEntity
    {
        public int SellerId { get; set; }
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

        public ListingStatus Status { get; set; } = ListingStatus.Active;
        public DateTime ExpiresAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? CompletedByUserId { get; set; }
        public DateTime? BlockedAt { get; set; }
        public int? BlockedByUserId { get; set; }
        public string? BlockReason { get; set; }
        public DateTime? WithdrawnAt { get; set; }

        public User Seller { get; set; } = null!;
        public Pet? Pet { get; set; }
        public ICollection<MarketplaceListingPhoto> Photos { get; set; } = new List<MarketplaceListingPhoto>();
        public OwnershipTransfer? OwnershipTransfer { get; set; }
    }
}
