using HappyPet.Domain.Entities;

namespace HappyPet.Application.Interfaces.Repositories
{
    public class ShelterSearchCriteria
    {
        public string? Search { get; set; }
        public string? City { get; set; }
    }

    public interface IShelterRepository
    {
        Task AddAsync(ShelterProfile profile, CancellationToken cancellationToken = default);
        void Update(ShelterProfile profile);

        Task<ShelterProfile?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ShelterProfile?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

        Task<bool> ExistsByRegistrationNumberAsync(
            string registrationNumber,
            int? excludeProfileId = null,
            CancellationToken cancellationToken = default);

        Task<(List<ShelterProfile> Items, int TotalCount)> GetVerifiedPagedAsync(
            ShelterSearchCriteria criteria,
            int skip,
            int take,
            CancellationToken cancellationToken = default);

        Task<(List<ShelterProfile> Items, int TotalCount)> GetPagedAsync(
            ShelterSearchCriteria criteria,
            bool? verified,
            int skip,
            int take,
            CancellationToken cancellationToken = default);

        Task<int> CountAsync(bool? verified = null, CancellationToken cancellationToken = default);
    }
}
