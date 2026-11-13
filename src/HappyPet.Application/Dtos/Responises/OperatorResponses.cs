using HappyPet.Domain.Enums;

namespace HappyPet.Application.Dtos.Responses
{
    public class OperatorTransferResponse
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public int? PetId { get; set; }
        public string? MicrochipNumber { get; set; }

        public string PetName { get; set; } = string.Empty;
        public PetSpecies Species { get; set; }
        public ListingType ListingType { get; set; }
        public decimal? Price { get; set; }
        public string Currency { get; set; } = "GEL";
        public DateTime? ListingCompletedAt { get; set; }

        public int SellerId { get; set; }
        public string SellerFullName { get; set; } = string.Empty;

        public int? BuyerUserId { get; set; }
        public string BuyerFullName { get; set; } = string.Empty;
        public string BuyerPersonalId { get; set; } = string.Empty;
        public string? BuyerPhone { get; set; }
        public string? BuyerAddress { get; set; }

        public OwnershipTransferType TransferType { get; set; }
        public OwnershipTransferStatus Status { get; set; }
        public DateTime TransferredAt { get; set; }
        public DateTime? AirsSyncedAt { get; set; }
        public string? AirsReference { get; set; }

        public int? ProcessedByUserId { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? RejectionReason { get; set; }
        public string? OperatorNotes { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OperatorDashboardResponse
    {
        public int PendingTransfers { get; set; }
        public int SyncedTransfers { get; set; }
        public int RejectedTransfers { get; set; }
        public int TotalTransfers { get; set; }
    }
}
