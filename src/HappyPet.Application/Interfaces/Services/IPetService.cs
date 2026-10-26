using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;

namespace HappyPet.Application.Interfaces.Services
{
    public interface IPetService
    {
        Task<PetResponse> CreateAsync(Guid ownerId, CreatePetRequest request, CancellationToken cancellationToken = default);
        Task<PetResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<PetResponse>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
        Task<List<PetResponse>> GetLostAsync(CancellationToken cancellationToken = default);
        Task<PetResponse> UpdateAsync(Guid id, UpdatePetRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        Task<List<PetPhotoResponse>> GetPhotosAsync(Guid petId, CancellationToken cancellationToken = default);
        Task<PetPhotoResponse> AddPhotoAsync(Guid petId, Stream stream, long length, string contentType, CancellationToken cancellationToken = default);
        Task<PetPhotoResponse> SetPrimaryPhotoAsync(Guid petId, Guid photoId, CancellationToken cancellationToken = default);
        Task DeletePhotoAsync(Guid petId, Guid photoId, CancellationToken cancellationToken = default);
    }
}
