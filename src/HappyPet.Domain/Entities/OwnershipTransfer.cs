using HappyPet.Domain.Common;
using HappyPet.Domain.Enums;

namespace HappyPet.Domain.Entities
{
    /// <summary>
    /// Local record of an ownership change completed through the marketplace.
    /// The actual transfer in AIRS is performed later by an operator or clinic;
    /// until then the record stays in <see cref="OwnershipTransferStatus.PendingAirsSync"/>.
    /// </summary>
    public class OwnershipTransfer : BaseEntity
    {
        public int ListingId { get; set; }
        public int? PetId { get; set; }
        public string? MicrochipNumber { get; set; }

        public int SellerId { get; set; }
        public int? BuyerUserId { get; set; }
        public string BuyerFullName { get; set; } = string.Empty;
        public string BuyerPersonalId { get; set; } = string.Empty;
        public string? BuyerPhone { get; set; }
        public string? BuyerAddress { get; set; }

        public OwnershipTransferType TransferType { get; set; }
        public decimal? Price { get; set; }
        public OwnershipTransferStatus Status { get; set; } = OwnershipTransferStatus.PendingAirsSync;
        public DateTime TransferredAt { get; set; }
        public DateTime? AirsSyncedAt { get; set; }
        public string? AirsReference { get; set; }

        public int? ProcessedByUserId { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? RejectionReason { get; set; }
        public string? OperatorNotes { get; set; }
        public string? Notes { get; set; }

        public MarketplaceListing Listing { get; set; } = null!;
    }
}
