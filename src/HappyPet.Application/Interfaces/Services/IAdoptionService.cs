using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Domain.Enums;

namespace HappyPet.Application.Interfaces.Services
{
    public interface IAdoptionService
    {
        Task<AdoptionListingResponse> CreateListingAsync(
            int shelterUserId,
            CreateAdoptionListingRequest request,
            CancellationToken cancellationToken = default);

        Task<AdoptionListingResponse?> GetListingAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<AdoptionListingResponse?> GetPublicListingAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<PagedResult<AdoptionListingResponse>> GetActiveListingsAsync(
            AdoptionSearchCriteria criteria,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        Task<PagedResult<AdoptionListingResponse>> GetShelterListingsAsync(
            int shelterUserId,
            AdoptionListingStatus? status = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        Task<AdoptionListingResponse> UpdateListingAsync(
            int id,
            int shelterUserId,
            UpdateAdoptionListingRequest request,
            CancellationToken cancellationToken = default);

        Task<AdoptionListingResponse> WithdrawListingAsync(
            int id,
            int shelterUserId,
            CancellationToken cancellationToken = default);

        Task<AdoptionListingResponse> PauseListingAsync(
            int id,
            int shelterUserId,
            CancellationToken cancellationToken = default);

        Task<AdoptionListingResponse> ResumeListingAsync(
            int id,
            int shelterUserId,
            CancellationToken cancellationToken = default);

        Task<AdoptionApplicationResponse> ApplyAsync(
            int listingId,
            int applicantUserId,
            CreateAdoptionApplicationRequest request,
            CancellationToken cancellationToken = default);

        Task<AdoptionApplicationResponse?> GetApplicationAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<PagedResult<AdoptionApplicationResponse>> GetListingApplicationsAsync(
            int listingId,
            int shelterUserId,
            AdoptionApplicationStatus? status = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        Task<PagedResult<AdoptionApplicationResponse>> GetMyApplicationsAsync(
            int applicantUserId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        Task<AdoptionApplicationResponse> ApproveApplicationAsync(
            int applicationId,
            int shelterUserId,
            ReviewAdoptionApplicationRequest request,
            CancellationToken cancellationToken = default);

        Task<AdoptionApplicationResponse> RejectApplicationAsync(
            int applicationId,
            int shelterUserId,
            ReviewAdoptionApplicationRequest request,
            CancellationToken cancellationToken = default);

        Task<AdoptionApplicationResponse> CompleteApplicationAsync(
            int applicationId,
            int shelterUserId,
            ReviewAdoptionApplicationRequest request,
            CancellationToken cancellationToken = default);

        Task<AdoptionApplicationResponse> WithdrawApplicationAsync(
            int applicationId,
            int applicantUserId,
            CancellationToken cancellationToken = default);
    }
}
