using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Domain.Entities;
using HappyPet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HappyPet.Infrastructure.Persistence.Repositories
{
    public class VaccinationRepository : IVaccinationRepository
    {
        private readonly HappyPetDbContext _context;

        public VaccinationRepository(HappyPetDbContext context)
        {
            _context = context;
        }

        public async Task<List<Vaccination>> GetByPetIdAsync(int petId, CancellationToken cancellationToken = default)
        {
            return await _context.Vaccinations
                .AsNoTracking()
                .Where(v => v.PetId == petId)
                .OrderByDescending(v => v.VaccinatedAt)
                .ThenByDescending(v => v.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<Vaccination?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Vaccinations
                .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        }

        public async Task<List<Vaccination>> GetDueAsync(
            int ownerId,
            DateTime toUtc,
            CancellationToken cancellationToken = default)
        {
            return await _context.Vaccinations
                .AsNoTracking()
                .Include(v => v.Pet)
                .Where(v => v.Pet.OwnerId == ownerId
                            && v.NextDueAt != null
                            && v.NextDueAt <= toUtc)
                .OrderBy(v => v.NextDueAt)
                .ThenBy(v => v.Pet.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAirsVaccinationAsync(
            int petId,
            string airsVaccinationId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Vaccinations
                .AnyAsync(
                    v => v.PetId == petId && v.AirsVaccinationId == airsVaccinationId,
                    cancellationToken);
        }

        public async Task AddAsync(Vaccination vaccination, CancellationToken cancellationToken = default)
        {
            await _context.Vaccinations.AddAsync(vaccination, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<Vaccination> vaccinations, CancellationToken cancellationToken = default)
        {
            await _context.Vaccinations.AddRangeAsync(vaccinations, cancellationToken);
        }

        public void Update(Vaccination vaccination)
        {
            _context.Vaccinations.Update(vaccination);
        }

        public void Delete(Vaccination vaccination)
        {
            _context.Vaccinations.Remove(vaccination);
        }
    }
}
