using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyPet.Infrastructure.Persistence.Repositories
{
    public class MarketplaceRepository : IMarketplaceRepository
    {
        private readonly HappyPetDbContext _context;

        public MarketplaceRepository(HappyPetDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(MarketplaceListing listing, CancellationToken cancellationToken = default)
        {
            await _context.MarketplaceListings.AddAsync(listing, cancellationToken);
        }

        public void Update(MarketplaceListing listing)
        {
            _context.MarketplaceListings.Update(listing);
        }

        public async Task<MarketplaceListing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.MarketplaceListings
                .Include(l => l.Photos)
                .Include(l => l.OwnershipTransfer)
                .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        }

        public async Task<(List<MarketplaceListing> Items, int TotalCount)> GetActivePagedAsync(
            MarketplaceSearchCriteria criteria,
            DateTime nowUtc,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = _context.MarketplaceListings
                .AsNoTracking()
                .Where(l => l.Status == ListingStatus.Active && l.ExpiresAt > nowUtc);

            if (criteria.Species.HasValue)
            {
                query = query.Where(l => l.Species == criteria.Species.Value);
            }

            if (criteria.ListingType.HasValue)
            {
                query = query.Where(l => l.ListingType == criteria.ListingType.Value);
            }

            if (!string.IsNullOrWhiteSpace(criteria.City))
            {
                var city = criteria.City.Trim();
                query = query.Where(l => EF.Functions.ILike(l.City, city));
            }

            if (criteria.MinPrice.HasValue)
            {
                query = query.Where(l => l.Price >= criteria.MinPrice.Value);
            }

            if (criteria.MaxPrice.HasValue)
            {
                query = query.Where(l => l.Price <= criteria.MaxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(criteria.Search))
            {
                var search = $"%{criteria.Search.Trim()}%";
                query = query.Where(l =>
                    EF.Functions.ILike(l.PetName, search)
                    || (l.Breed != null && EF.Functions.ILike(l.Breed, search))
                    || (l.Description != null && EF.Functions.ILike(l.Description, search)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Include(l => l.Photos)
                .OrderByDescending(l => l.CreatedAt)
                .ThenByDescending(l => l.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<(List<MarketplaceListing> Items, int TotalCount)> GetBySellerPagedAsync(
            int sellerId,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = _context.MarketplaceListings
                .AsNoTracking()
                .Where(l => l.SellerId == sellerId);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Include(l => l.Photos)
                .OrderByDescending(l => l.CreatedAt)
                .ThenByDescending(l => l.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<bool> HasActiveListingForPetAsync(
            int petId,
            int? excludeListingId = null,
            CancellationToken cancellationToken = default)
        {
            return await _context.MarketplaceListings
                .AnyAsync(l => l.PetId == petId
                               && l.Status == ListingStatus.Active
                               && (!excludeListingId.HasValue || l.Id != excludeListingId.Value),
                    cancellationToken);
        }

        public async Task<int> ExpireActiveAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
        {
            return await _context.MarketplaceListings
                .Where(l => l.Status == ListingStatus.Active && l.ExpiresAt <= nowUtc)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(l => l.Status, ListingStatus.Expired)
                    .SetProperty(l => l.UpdatedAt, nowUtc), cancellationToken);
        }

        public async Task AddPhotoAsync(MarketplaceListingPhoto photo, CancellationToken cancellationToken = default)
        {
            await _context.MarketplaceListingPhotos.AddAsync(photo, cancellationToken);
        }

        public async Task<MarketplaceListingPhoto?> GetPhotoByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.MarketplaceListingPhotos
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<List<MarketplaceListingPhoto>> GetPhotosByListingIdAsync(
            int listingId,
            CancellationToken cancellationToken = default)
        {
            return await _context.MarketplaceListingPhotos
                .Where(p => p.ListingId == listingId)
                .OrderByDescending(p => p.IsPrimary)
                .ThenBy(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountPhotosByListingIdAsync(int listingId, CancellationToken cancellationToken = default)
        {
            return await _context.MarketplaceListingPhotos
                .CountAsync(p => p.ListingId == listingId, cancellationToken);
        }

        public void DeletePhoto(MarketplaceListingPhoto photo)
        {
            _context.MarketplaceListingPhotos.Remove(photo);
        }

        public async Task AddOwnershipTransferAsync(OwnershipTransfer transfer, CancellationToken cancellationToken = default)
        {
            await _context.OwnershipTransfers.AddAsync(transfer, cancellationToken);
        }

        public async Task<OwnershipTransfer?> GetOwnershipTransferByListingIdAsync(
            int listingId,
            CancellationToken cancellationToken = default)
        {
            return await _context.OwnershipTransfers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ListingId == listingId, cancellationToken);
        }
    }
}
