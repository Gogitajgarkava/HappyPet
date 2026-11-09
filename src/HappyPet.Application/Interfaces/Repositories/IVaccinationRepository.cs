using HappyPet.Domain.Entities;

namespace HappyPet.Application.Interfaces.Repositories
{
    public interface IVaccinationRepository
    {
        Task<List<Vaccination>> GetByPetIdAsync(int petId, CancellationToken cancellationToken = default);
        Task<Vaccination?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Vaccination>> GetDueAsync(int ownerId, DateTime toUtc, CancellationToken cancellationToken = default);
        Task<bool> ExistsAirsVaccinationAsync(int petId, string airsVaccinationId, CancellationToken cancellationToken = default);
        Task AddAsync(Vaccination vaccination, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<Vaccination> vaccinations, CancellationToken cancellationToken = default);
        void Update(Vaccination vaccination);
        void Delete(Vaccination vaccination);
    }
}
