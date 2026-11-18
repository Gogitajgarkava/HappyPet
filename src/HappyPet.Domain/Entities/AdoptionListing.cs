using HappyPet.Domain.Common;
using HappyPet.Domain.Enums;

namespace HappyPet.Domain.Entities
{
    /// <summary>
    /// An adoption listing published by a verified shelter for one of its pets.
    /// Pet details and photos are read from the linked <see cref="Pet"/> so the
    /// listing always stays in sync with the registered animal profile.
    /// </summary>
    public class AdoptionListing : BaseEntity
    {
        public int ShelterId { get; set; }
        public int PetId { get; set; }

        public string? Description { get; set; }
        public string City { get; set; } = string.Empty;
        public decimal? AdoptionFee { get; set; }
        public bool IsUrgent { get; set; }

        public AdoptionListingStatus Status { get; set; } = AdoptionListingStatus.Active;
        public DateTime PublishedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public User Shelter { get; set; } = null!;
        public Pet Pet { get; set; } = null!;
        public ICollection<AdoptionApplication> Applications { get; set; } = new List<AdoptionApplication>();
    }
}
