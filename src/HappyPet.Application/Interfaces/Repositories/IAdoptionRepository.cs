using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;

namespace HappyPet.Application.Interfaces.Repositories
{
    public class AdoptionSearchCriteria
    {
        public PetSpecies? Species { get; set; }
        public string? City { get; set; }
        public string? Search { get; set; }
        public int? ShelterId { get; set; }
    }

    public sealed class ShelterAdoptionStats
    {
        public int TotalListings { get; init; }
        public int ActiveListings { get; init; }
        public int PausedListings { get; init; }
        public int AdoptedListings { get; init; }
        public int WithdrawnListings { get; init; }
        public int TotalApplications { get; init; }
        public int PendingApplications { get; init; }
        public int ApprovedApplications { get; init; }
        public int CompletedAdoptions { get; init; }
    }

    public sealed class AdminAdoptionStats
    {
        public int ActiveListings { get; init; }
        public int PendingApplications { get; init; }
        public int CompletedAdoptions { get; init; }
    }

    public interface IAdoptionRepository
    {
        Task AddListingAsync(AdoptionListing listing, CancellationToken cancellationToken = default);
        void UpdateListing(AdoptionListing listing);

        Task<AdoptionListing?> GetListingByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<(List<AdoptionListing> Items, int TotalCount)> GetActiveListingsPagedAsync(
            AdoptionSearchCriteria criteria,
            int skip,
            int take,
            CancellationToken cancellationToken = default);
        Task<(List<AdoptionListing> Items, int TotalCount)> GetListingsByShelterPagedAsync(
            int shelterId,
            AdoptionListingStatus? status,
            int skip,
            int take,
            CancellationToken cancellationToken = default);
        Task<bool> HasActiveListingForPetAsync(
            int petId,
            int? excludeListingId = null,
            CancellationToken cancellationToken = default);

        Task AddApplicationAsync(AdoptionApplication application, CancellationToken cancellationToken = default);
        void UpdateApplication(AdoptionApplication application);

        Task<AdoptionApplication?> GetApplicationByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<AdoptionApplication?> GetApplicationByListingAndApplicantAsync(
            int listingId,
            int applicantUserId,
            CancellationToken cancellationToken = default);
        Task<List<AdoptionApplication>> GetApplicationsByListingAsync(
            int listingId,
            CancellationToken cancellationToken = default);
        Task<(List<AdoptionApplication> Items, int TotalCount)> GetApplicationsByListingPagedAsync(
            int listingId,
            AdoptionApplicationStatus? status,
            int skip,
            int take,
            CancellationToken cancellationToken = default);
        Task<(List<AdoptionApplication> Items, int TotalCount)> GetApplicationsByApplicantPagedAsync(
            int applicantUserId,
            int skip,
            int take,
            CancellationToken cancellationToken = default);

        Task<ShelterAdoptionStats> GetShelterStatsAsync(
            int shelterId,
            CancellationToken cancellationToken = default);
        Task<AdminAdoptionStats> GetAdminStatsAsync(CancellationToken cancellationToken = default);
    }
}
