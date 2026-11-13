using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;

namespace HappyPet.Application.Interfaces.Services
{
    public interface IVaccinationService
    {
        Task<List<VaccinationResponse>> GetByPetIdAsync(int petId, CancellationToken cancellationToken = default);
        Task<List<VaccinationResponse>> GetDueAsync(
            int ownerId,
            int daysAhead = 30,
            CancellationToken cancellationToken = default);
        Task<VaccinationResponse> CreateAsync(
            int petId,
            CreateVaccinationRequest request,
            CancellationToken cancellationToken = default,
            bool allowNextDueOverride = false,
            bool allowEarlyVaccination = false);
        Task<VaccinationResponse> UpdateAsync(
            int petId,
            int vaccinationId,
            UpdateVaccinationRequest request,
            CancellationToken cancellationToken = default,
            bool allowNextDueOverride = false);
        Task DeleteAsync(int petId, int vaccinationId, CancellationToken cancellationToken = default);
        Task<VaccinationImportResult> ImportFromAirsAsync(int petId, CancellationToken cancellationToken = default);
    }
}
