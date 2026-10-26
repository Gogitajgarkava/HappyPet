using HappyPet.Domain.Entities;

namespace HappyPet.Application.Interfaces.Repositories
{
    public interface IPetRepository
    {
        Task<Pet?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Pet>> GetByOwnerIdAsync(int ownerId, CancellationToken cancellationToken = default);
        Task<List<Pet>> GetLostAsync(CancellationToken cancellationToken = default);

        Task<bool> ExistsByMicrochipNumberAsync(string microchipNumber, CancellationToken cancellationToken = default);

        Task AddAsync(Pet pet, CancellationToken cancellationToken = default);
        void Update(Pet pet);
        void Delete(Pet pet);

        Task<PetPhoto?> GetPhotoByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<PetPhoto>> GetPhotosByPetIdAsync(int petId, CancellationToken cancellationToken = default);
        Task<int> CountPhotosByPetIdAsync(int petId, CancellationToken cancellationToken = default);
        Task AddPhotoAsync(PetPhoto photo, CancellationToken cancellationToken = default);
        void DeletePhoto(PetPhoto photo);
    }
}
