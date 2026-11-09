using HappyPet.Domain.Entities;

namespace HappyPet.Application.Interfaces.Repositories
{
    public interface IPetRepository
    {
        Task<Pet?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<(List<Pet> Items, int TotalCount)> GetByOwnerIdPagedAsync(int ownerId, int skip, int take, CancellationToken cancellationToken = default);
        Task<(List<Pet> Items, int TotalCount)> GetLostPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
        Task<List<Pet>> GetLostWithinBoundingBoxAsync(
            double minLatitude,
            double maxLatitude,
            double minLongitude,
            double maxLongitude,
            bool crossesAntimeridian,
            CancellationToken cancellationToken = default);

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
