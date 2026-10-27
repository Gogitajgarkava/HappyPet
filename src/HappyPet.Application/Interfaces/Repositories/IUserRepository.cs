using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;

namespace HappyPet.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetByPersonalIdAsync(string personalId, CancellationToken cancellationToken = default);

        Task<User?> GetWithPetsAsync(int id, CancellationToken cancellationToken = default);

        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsByPersonalIdAsync(string personalId, CancellationToken cancellationToken = default);

        Task AddAsync(User user, CancellationToken cancellationToken = default);
        void Update(User user);
        void Delete(User user);

        Task AddOtpCodeAsync(UserOtpCode code, CancellationToken cancellationToken = default);
        Task<UserOtpCode?> GetLatestOtpCodeAsync(int userId, OtpPurpose purpose, CancellationToken cancellationToken = default);
        Task<List<UserOtpCode>> GetActiveOtpCodesAsync(int userId, OtpPurpose purpose, CancellationToken cancellationToken = default);
        Task<int> CountOtpCodesSinceAsync(int userId, OtpPurpose purpose, DateTime sinceUtc, CancellationToken cancellationToken = default);
        void UpdateOtpCode(UserOtpCode code);
    }
}
