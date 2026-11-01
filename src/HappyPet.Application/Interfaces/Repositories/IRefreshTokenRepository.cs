using HappyPet.Domain.Entities;

namespace HappyPet.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);
        Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
        Task<RefreshToken?> GetByHashWithUserAsync(string tokenHash, CancellationToken cancellationToken = default);
        Task<List<RefreshToken>> GetActiveByUserAsync(int userId, DateTime now, CancellationToken cancellationToken = default);
        Task<List<RefreshToken>> GetActiveByFamilyAsync(Guid familyId, DateTime now, CancellationToken cancellationToken = default);
        void Update(RefreshToken token);
        Task<int> DeleteExpiredAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default);
    }
}
