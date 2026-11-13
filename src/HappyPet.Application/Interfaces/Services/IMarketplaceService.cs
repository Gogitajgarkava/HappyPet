using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;
using HappyPet.Application.Interfaces.Repositories;

namespace HappyPet.Application.Interfaces.Services
{
    public interface IMarketplaceService
    {
        Task<MarketplaceListingResponse> CreateAsync(
            int sellerId,
            CreateMarketplaceListingRequest request,
            CancellationToken cancellationToken = default);

        Task<MarketplaceListingResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<PagedResult<MarketplaceListingResponse>> GetActiveAsync(
            MarketplaceSearchCriteria criteria,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        Task<PagedResult<MarketplaceListingResponse>> GetBySellerAsync(
            int sellerId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        Task<MarketplaceListingResponse> UpdateAsync(
            int id,
            UpdateMarketplaceListingRequest request,
            CancellationToken cancellationToken = default);

        Task WithdrawAsync(int id, CancellationToken cancellationToken = default);

        Task<CompleteMarketplaceListingResponse> CompleteAsync(
            int id,
            int completedByUserId,
            CompleteMarketplaceListingRequest request,
            CancellationToken cancellationToken = default);

        Task<MarketplaceListingResponse> BlockAsync(
            int id,
            int adminUserId,
            BlockMarketplaceListingRequest request,
            CancellationToken cancellationToken = default);

        Task<MarketplaceListingResponse> UnblockAsync(
            int id,
            int adminUserId,
            CancellationToken cancellationToken = default);

        Task<List<MarketplaceListingPhotoResponse>> GetPhotosAsync(
            int listingId,
            CancellationToken cancellationToken = default);

        Task<MarketplaceListingPhotoResponse> AddPhotoAsync(
            int listingId,
            Stream stream,
            long length,
            string contentType,
            CancellationToken cancellationToken = default);

        Task<MarketplaceListingPhotoResponse> SetPrimaryPhotoAsync(
            int listingId,
            int photoId,
            CancellationToken cancellationToken = default);

        Task DeletePhotoAsync(
            int listingId,
            int photoId,
            CancellationToken cancellationToken = default);
    }
}
