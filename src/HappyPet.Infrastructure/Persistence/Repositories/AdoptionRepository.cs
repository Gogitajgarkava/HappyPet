using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyPet.Infrastructure.Persistence.Repositories
{
    public class AdoptionRepository : IAdoptionRepository
    {
        private readonly HappyPetDbContext _context;

        public AdoptionRepository(HappyPetDbContext context)
        {
            _context = context;
        }

        public async Task AddListingAsync(AdoptionListing listing, CancellationToken cancellationToken = default)
        {
            await _context.AdoptionListings.AddAsync(listing, cancellationToken);
        }

        public void UpdateListing(AdoptionListing listing)
        {
            _context.AdoptionListings.Update(listing);
        }

        public async Task<AdoptionListing?> GetListingByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await ListingQuery(tracking: true)
                .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        }

        public async Task<(List<AdoptionListing> Items, int TotalCount)> GetActiveListingsPagedAsync(
            AdoptionSearchCriteria criteria,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = ListingQuery(tracking: false)
                .Where(l => l.Status == AdoptionListingStatus.Active
                            && l.Shelter.ShelterProfile != null
                            && l.Shelter.ShelterProfile.IsVerified);

            if (criteria.Species.HasValue)
            {
                query = query.Where(l => l.Pet.Species == criteria.Species.Value);
            }

            if (criteria.ShelterId.HasValue)
            {
                query = query.Where(l => l.ShelterId == criteria.ShelterId.Value);
            }

            if (!string.IsNullOrWhiteSpace(criteria.City))
            {
                var city = criteria.City.Trim();
                query = query.Where(l => EF.Functions.ILike(l.City, city));
            }

            if (!string.IsNullOrWhiteSpace(criteria.Search))
            {
                var search = $"%{criteria.Search.Trim()}%";
                query = query.Where(l =>
                    EF.Functions.ILike(l.Pet.Name, search)
                    || (l.Pet.Breed != null && EF.Functions.ILike(l.Pet.Breed, search))
                    || (l.Description != null && EF.Functions.ILike(l.Description, search))
                    || EF.Functions.ILike(l.Shelter.ShelterProfile!.OrganizationName, search));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(l => l.IsUrgent)
                .ThenByDescending(l => l.PublishedAt)
                .ThenByDescending(l => l.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<(List<AdoptionListing> Items, int TotalCount)> GetListingsByShelterPagedAsync(
            int shelterId,
            AdoptionListingStatus? status,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = ListingQuery(tracking: false)
                .Where(l => l.ShelterId == shelterId);

            if (status.HasValue)
            {
                query = query.Where(l => l.Status == status.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(l => l.PublishedAt)
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
            return await _context.AdoptionListings
                .AnyAsync(
                    l => l.PetId == petId
                         && (l.Status == AdoptionListingStatus.Active
                             || l.Status == AdoptionListingStatus.Paused)
                         && (!excludeListingId.HasValue || l.Id != excludeListingId.Value),
                    cancellationToken);
        }

        public async Task AddApplicationAsync(
            AdoptionApplication application,
            CancellationToken cancellationToken = default)
        {
            await _context.AdoptionApplications.AddAsync(application, cancellationToken);
        }

        public void UpdateApplication(AdoptionApplication application)
        {
            _context.AdoptionApplications.Update(application);
        }

        public async Task<AdoptionApplication?> GetApplicationByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await ApplicationQuery(tracking: true)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<AdoptionApplication?> GetApplicationByListingAndApplicantAsync(
            int listingId,
            int applicantUserId,
            CancellationToken cancellationToken = default)
        {
            return await ApplicationQuery(tracking: true)
                .FirstOrDefaultAsync(
                    a => a.ListingId == listingId && a.ApplicantUserId == applicantUserId,
                    cancellationToken);
        }

        public async Task<List<AdoptionApplication>> GetApplicationsByListingAsync(
            int listingId,
            CancellationToken cancellationToken = default)
        {
            return await ApplicationQuery(tracking: true)
                .Where(a => a.ListingId == listingId)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<AdoptionApplication> Items, int TotalCount)> GetApplicationsByListingPagedAsync(
            int listingId,
            AdoptionApplicationStatus? status,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = ApplicationQuery(tracking: false)
                .Where(a => a.ListingId == listingId);

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<(List<AdoptionApplication> Items, int TotalCount)> GetApplicationsByApplicantPagedAsync(
            int applicantUserId,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = ApplicationQuery(tracking: false)
                .Where(a => a.ApplicantUserId == applicantUserId);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<ShelterAdoptionStats> GetShelterStatsAsync(
            int shelterId,
            CancellationToken cancellationToken = default)
        {
            var listings = _context.AdoptionListings.Where(l => l.ShelterId == shelterId);
            var applications = _context.AdoptionApplications
                .Where(a => a.Listing.ShelterId == shelterId);

            return new ShelterAdoptionStats
            {
                TotalListings = await listings.CountAsync(cancellationToken),
                ActiveListings = await listings.CountAsync(
                    l => l.Status == AdoptionListingStatus.Active,
                    cancellationToken),
                PausedListings = await listings.CountAsync(
                    l => l.Status == AdoptionListingStatus.Paused,
                    cancellationToken),
                AdoptedListings = await listings.CountAsync(
                    l => l.Status == AdoptionListingStatus.Adopted,
                    cancellationToken),
                WithdrawnListings = await listings.CountAsync(
                    l => l.Status == AdoptionListingStatus.Withdrawn,
                    cancellationToken),
                TotalApplications = await applications.CountAsync(cancellationToken),
                PendingApplications = await applications.CountAsync(
                    a => a.Status == AdoptionApplicationStatus.Pending,
                    cancellationToken),
                ApprovedApplications = await applications.CountAsync(
                    a => a.Status == AdoptionApplicationStatus.Approved,
                    cancellationToken),
                CompletedAdoptions = await applications.CountAsync(
                    a => a.Status == AdoptionApplicationStatus.Completed,
                    cancellationToken)
            };
        }

        public async Task<AdminAdoptionStats> GetAdminStatsAsync(CancellationToken cancellationToken = default)
        {
            return new AdminAdoptionStats
            {
                ActiveListings = await _context.AdoptionListings.CountAsync(
                    l => l.Status == AdoptionListingStatus.Active,
                    cancellationToken),
                PendingApplications = await _context.AdoptionApplications.CountAsync(
                    a => a.Status == AdoptionApplicationStatus.Pending,
                    cancellationToken),
                CompletedAdoptions = await _context.AdoptionApplications.CountAsync(
                    a => a.Status == AdoptionApplicationStatus.Completed,
                    cancellationToken)
            };
        }

        private IQueryable<AdoptionListing> ListingQuery(bool tracking)
        {
            var query = _context.AdoptionListings.AsQueryable();

            if (!tracking)
            {
                query = query.AsNoTracking();
            }

            return query
                .Include(l => l.Pet)
                .ThenInclude(p => p.Photos)
                .Include(l => l.Shelter)
                .ThenInclude(u => u.ShelterProfile);
        }

        private IQueryable<AdoptionApplication> ApplicationQuery(bool tracking)
        {
            var query = _context.AdoptionApplications.AsQueryable();

            if (!tracking)
            {
                query = query.AsNoTracking();
            }

            return query
                .Include(a => a.Applicant)
                .Include(a => a.Listing)
                .ThenInclude(l => l.Pet)
                .ThenInclude(p => p.Photos)
                .Include(a => a.Listing)
                .ThenInclude(l => l.Shelter)
                .ThenInclude(u => u.ShelterProfile);
        }
    }
}
