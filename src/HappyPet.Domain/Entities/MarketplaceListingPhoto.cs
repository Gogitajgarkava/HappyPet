using HappyPet.Domain.Common;

namespace HappyPet.Domain.Entities
{
    public class MarketplaceListingPhoto : BaseEntity
    {
        public int ListingId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public bool IsPrimary { get; set; }

        public MarketplaceListing Listing { get; set; } = null!;
    }
}
