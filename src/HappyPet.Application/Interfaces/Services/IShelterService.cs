using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;
using HappyPet.Application.Interfaces.Repositories;

namespace HappyPet.Application.Interfaces.Services
{
    public interface IShelterService
    {
        Task<ShelterProfileResponse> UpsertProfileAsync(
            int userId,
            UpdateShelterProfileRequest request,
            CancellationToken cancellationToken = default);

        Task<ShelterProfileResponse?> GetMyProfileAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<ShelterPublicResponse> GetPublicProfileAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<PagedResult<ShelterPublicResponse>> GetPublicListAsync(
            ShelterSearchCriteria criteria,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        Task<ShelterProfileResponse> VerifyAsync(
            int id,
            int adminUserId,
            CancellationToken cancellationToken = default);

        Task<ShelterProfileResponse> UnverifyAsync(
            int id,
            int adminUserId,
            CancellationToken cancellationToken = default);

        Task<ShelterDashboardResponse> GetDashboardAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<AdminShelterDashboardResponse> GetAdminDashboardAsync(
            CancellationToken cancellationToken = default);

        Task<PagedResult<ShelterProfileResponse>> GetAdminListAsync(
            ShelterSearchCriteria criteria,
            bool? verified = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        Task GrantShelterRoleAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}
