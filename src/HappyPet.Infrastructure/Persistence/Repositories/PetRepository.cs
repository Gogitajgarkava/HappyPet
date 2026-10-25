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

        public async Task<Pet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Pets
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<List<Pet>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
        {
            return await _context.Pets
                .Where(p => p.OwnerId == ownerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Pet>> GetLostAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Pets
                .Where(p => p.IsLost)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByMicrochipNumberAsync(string microchipNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Pets
                .AnyAsync(p => p.MicrochipNumber == microchipNumber, cancellationToken);
        }
    }
}
