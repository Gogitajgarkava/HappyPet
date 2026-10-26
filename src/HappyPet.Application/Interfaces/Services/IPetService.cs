using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;

namespace HappyPet.Application.Interfaces.Services
{
    public interface IPetService
    {
        Task<PetResponse> CreateAsync(int ownerId, CreatePetRequest request, CancellationToken cancellationToken = default);
        Task<PetResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<PetResponse>> GetByOwnerIdAsync(int ownerId, CancellationToken cancellationToken = default);
        Task<List<PetResponse>> GetLostAsync(CancellationToken cancellationToken = default);
        Task<PetResponse> UpdateAsync(int id, UpdatePetRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        Task<List<PetPhotoResponse>> GetPhotosAsync(int petId, CancellationToken cancellationToken = default);
        Task<PetPhotoResponse> AddPhotoAsync(int petId, Stream stream, long length, string contentType, CancellationToken cancellationToken = default);
        Task<PetPhotoResponse> SetPrimaryPhotoAsync(int petId, int photoId, CancellationToken cancellationToken = default);
        Task DeletePhotoAsync(int petId, int photoId, CancellationToken cancellationToken = default);
    }
}
