namespace HappyPet.Application.Dtos.Requests
{
    public class MarkOwnershipTransferSyncedRequest
    {
        /// <summary>AIRS transfer/reference number confirming the ownership change.</summary>
        public string AirsReference { get; set; } = string.Empty;

        /// <summary>When the ownership change was completed in AIRS. Defaults to now.</summary>
        public DateTime? SyncedAt { get; set; }

        /// <summary>
        /// Optional buyer account when the marketplace completion did not include one.
        /// Required for a transfer linked to a registered pet if no account can be matched
        /// by the buyer's personal ID.
        /// </summary>
        public int? BuyerUserId { get; set; }

        public string? BuyerAddress { get; set; }
        public string? Notes { get; set; }
    }

    public class RejectOwnershipTransferRequest
    {
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
