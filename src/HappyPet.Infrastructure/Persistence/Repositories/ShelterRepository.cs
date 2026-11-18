using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HappyPet.Infrastructure.Persistence.Repositories
{
    public class ShelterRepository : IShelterRepository
    {
        private readonly HappyPetDbContext _context;

        public ShelterRepository(HappyPetDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ShelterProfile profile, CancellationToken cancellationToken = default)
        {
            await _context.ShelterProfiles.AddAsync(profile, cancellationToken);
        }

        public void Update(ShelterProfile profile)
        {
            _context.ShelterProfiles.Update(profile);
        }

        public async Task<ShelterProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.ShelterProfiles
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<ShelterProfile?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.ShelterProfiles
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        }

        public async Task<bool> ExistsByRegistrationNumberAsync(
            string registrationNumber,
            int? excludeProfileId = null,
            CancellationToken cancellationToken = default)
        {
            return await _context.ShelterProfiles
                .AnyAsync(
                    s => s.RegistrationNumber == registrationNumber
                         && (!excludeProfileId.HasValue || s.Id != excludeProfileId.Value),
                    cancellationToken);
        }

        public async Task<(List<ShelterProfile> Items, int TotalCount)> GetVerifiedPagedAsync(
            ShelterSearchCriteria criteria,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            return await GetPagedAsync(criteria, true, skip, take, cancellationToken);
        }

        public async Task<(List<ShelterProfile> Items, int TotalCount)> GetPagedAsync(
            ShelterSearchCriteria criteria,
            bool? verified,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = _context.ShelterProfiles.AsNoTracking().AsQueryable();

            if (verified.HasValue)
            {
                query = query.Where(s => s.IsVerified == verified.Value);
            }

            if (!string.IsNullOrWhiteSpace(criteria.City))
            {
                var city = criteria.City.Trim();
                query = query.Where(s => EF.Functions.ILike(s.City, city));
            }

            if (!string.IsNullOrWhiteSpace(criteria.Search))
            {
                var search = $"%{criteria.Search.Trim()}%";
                query = query.Where(s =>
                    EF.Functions.ILike(s.OrganizationName, search)
                    || (s.Description != null && EF.Functions.ILike(s.Description, search))
                    || EF.Functions.ILike(s.City, search));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(s => s.CreatedAt)
                .ThenBy(s => s.OrganizationName)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<int> CountAsync(bool? verified = null, CancellationToken cancellationToken = default)
        {
            var query = _context.ShelterProfiles.AsQueryable();

            if (verified.HasValue)
            {
                query = query.Where(s => s.IsVerified == verified.Value);
            }

            return await query.CountAsync(cancellationToken);
        }
    }
}
