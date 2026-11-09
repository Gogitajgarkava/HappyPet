using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HappyPet.Infrastructure.Persistence.Repositories
{
    public class PetRepository : IPetRepository
    {
        private readonly HappyPetDbContext _context;

        public PetRepository(HappyPetDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Pet pet, CancellationToken cancellationToken = default)
        {
            await _context.Pets.AddAsync(pet, cancellationToken);
        }

        public void Update(Pet pet)
        {
            _context.Pets.Update(pet);
        }

        public void Delete(Pet pet)
        {
            _context.Pets.Remove(pet);
        }

        public async Task<Pet?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Pets
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<(List<Pet> Items, int TotalCount)> GetByOwnerIdPagedAsync(
            int ownerId,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Pets
                .AsNoTracking()
                .Where(p => p.OwnerId == ownerId);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Include(p => p.Photos)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<(List<Pet> Items, int TotalCount)> GetLostPagedAsync(
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Pets
                .AsNoTracking()
                .Where(p => p.IsLost);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Include(p => p.Photos)
                .OrderByDescending(p => p.LostAt ?? p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<List<Pet>> GetLostWithinBoundingBoxAsync(
            double minLatitude,
            double maxLatitude,
            double minLongitude,
            double maxLongitude,
            bool crossesAntimeridian,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Pets
                .AsNoTracking()
                .Where(p => p.IsLost
                            && p.LastSeenLatitude != null
                            && p.LastSeenLongitude != null
                            && p.LastSeenLatitude >= minLatitude
                            && p.LastSeenLatitude <= maxLatitude);

            query = crossesAntimeridian
                ? query.Where(p => p.LastSeenLongitude >= minLongitude || p.LastSeenLongitude <= maxLongitude)
                : query.Where(p => p.LastSeenLongitude >= minLongitude && p.LastSeenLongitude <= maxLongitude);

            return await query
                .Include(p => p.Photos)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByMicrochipNumberAsync(string microchipNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Pets
                .AnyAsync(p => p.MicrochipNumber == microchipNumber, cancellationToken);
        }

        public async Task<PetPhoto?> GetPhotoByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.PetPhotos
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<List<PetPhoto>> GetPhotosByPetIdAsync(int petId, CancellationToken cancellationToken = default)
        {
            return await _context.PetPhotos
                .Where(p => p.PetId == petId)
                .OrderByDescending(p => p.IsPrimary)
                .ThenBy(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountPhotosByPetIdAsync(int petId, CancellationToken cancellationToken = default)
        {
            return await _context.PetPhotos
                .CountAsync(p => p.PetId == petId, cancellationToken);
        }

        public async Task AddPhotoAsync(PetPhoto photo, CancellationToken cancellationToken = default)
        {
            await _context.PetPhotos.AddAsync(photo, cancellationToken);
        }

        public void DeletePhoto(PetPhoto photo)
        {
            _context.PetPhotos.Remove(photo);
        }
    }
}
