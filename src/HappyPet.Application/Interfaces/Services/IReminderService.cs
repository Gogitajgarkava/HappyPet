using HappyPet.Application.Dtos.Responses;

namespace HappyPet.Application.Interfaces.Services
{
    public interface IReminderService
    {
        Task<PagedResult<VaccinationReminderResponse>> GetMyAsync(
            int ownerUserId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        Task<PagedResult<VaccinationReminderResponse>> GetByPetAsync(
            int petId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        Task<ReminderRunResponse> RunAsync(CancellationToken cancellationToken = default);
    }
}
