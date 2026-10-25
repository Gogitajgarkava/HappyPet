using HappyPet.Domain.Entities;

namespace HappyPet.Application.Interfaces.Repositories
{
    public interface IPetRepository
    {
        Task<Pet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Pet>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
        Task<List<Pet>> GetLostAsync(CancellationToken cancellationToken = default);

        Task<bool> ExistsByMicrochipNumberAsync(string microchipNumber, CancellationToken cancellationToken = default);

        Task AddAsync(Pet pet, CancellationToken cancellationToken = default);
        void Update(Pet pet);
        void Delete(Pet pet);
    }
}
