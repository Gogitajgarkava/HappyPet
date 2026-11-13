using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;

namespace HappyPet.Application.Interfaces.Services
{
    public interface IPetService
    {
        Task<PetResponse> CreateAsync(int ownerId, CreatePetRequest request, CancellationToken cancellationToken = default);
        Task<PetResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedResult<PetResponse>> GetByOwnerIdAsync(int ownerId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<PagedResult<LostPetResponse>> GetLostAsync(
            int page = 1,
            int pageSize = 20,
            double? latitude = null,
            double? longitude = null,
            double? radiusKm = null,
            CancellationToken cancellationToken = default);
        Task<PetResponse> ReportLostAsync(int id, ReportLostPetRequest request, CancellationToken cancellationToken = default);
        Task<PetResponse> ReportFoundAsync(int id, CancellationToken cancellationToken = default);
        Task<PetResponse> MarkDeceasedAsync(int id, MarkPetDeceasedRequest request, CancellationToken cancellationToken = default);
        Task<PetResponse> UpdateAsync(int id, UpdatePetRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        Task<List<PetPhotoResponse>> GetPhotosAsync(int petId, CancellationToken cancellationToken = default);
        Task<PetPhotoResponse> AddPhotoAsync(int petId, Stream stream, long length, string contentType, CancellationToken cancellationToken = default);
        Task<PetPhotoResponse> SetPrimaryPhotoAsync(int petId, int photoId, CancellationToken cancellationToken = default);
        Task DeletePhotoAsync(int petId, int photoId, CancellationToken cancellationToken = default);
    }
}
