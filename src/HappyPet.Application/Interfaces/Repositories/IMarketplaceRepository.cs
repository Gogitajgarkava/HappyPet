using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;

namespace HappyPet.Application.Interfaces.Repositories
{
    public class MarketplaceSearchCriteria
    {
        public PetSpecies? Species { get; set; }
        public ListingType? ListingType { get; set; }
        public string? City { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Search { get; set; }
    }

    public sealed class OwnershipTransferStats
    {
        public int PendingCount { get; init; }
        public int SyncedCount { get; init; }
        public int RejectedCount { get; init; }
        public int TotalCount { get; init; }
    }

    public interface IMarketplaceRepository
    {
        Task AddAsync(MarketplaceListing listing, CancellationToken cancellationToken = default);
        void Update(MarketplaceListing listing);

        Task<MarketplaceListing?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<(List<MarketplaceListing> Items, int TotalCount)> GetActivePagedAsync(
            MarketplaceSearchCriteria criteria,
            DateTime nowUtc,
            int skip,
            int take,
            CancellationToken cancellationToken = default);
        Task<(List<MarketplaceListing> Items, int TotalCount)> GetBySellerPagedAsync(
            int sellerId,
            int skip,
            int take,
            CancellationToken cancellationToken = default);

        Task<bool> HasActiveListingForPetAsync(
            int petId,
            int? excludeListingId = null,
            CancellationToken cancellationToken = default);

        Task<int> ExpireActiveAsync(DateTime nowUtc, CancellationToken cancellationToken = default);

        Task AddPhotoAsync(MarketplaceListingPhoto photo, CancellationToken cancellationToken = default);
        Task<MarketplaceListingPhoto?> GetPhotoByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<MarketplaceListingPhoto>> GetPhotosByListingIdAsync(
            int listingId,
            CancellationToken cancellationToken = default);
        Task<int> CountPhotosByListingIdAsync(int listingId, CancellationToken cancellationToken = default);
        void DeletePhoto(MarketplaceListingPhoto photo);

        Task AddOwnershipTransferAsync(OwnershipTransfer transfer, CancellationToken cancellationToken = default);
        Task<OwnershipTransfer?> GetOwnershipTransferByListingIdAsync(
            int listingId,
            CancellationToken cancellationToken = default);
        Task<OwnershipTransfer?> GetOwnershipTransferByIdAsync(
            int id,
            CancellationToken cancellationToken = default);
        Task<(List<OwnershipTransfer> Items, int TotalCount)> GetOwnershipTransfersPagedAsync(
            OwnershipTransferStatus? status,
            string? search,
            int skip,
            int take,
            CancellationToken cancellationToken = default);
        Task<OwnershipTransferStats> GetOwnershipTransferStatsAsync(
            CancellationToken cancellationToken = default);
    }
}
