using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HappyPet.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly HappyPetDbContext _context;

        public RefreshTokenRepository(HappyPetDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            await _context.RefreshTokens.AddAsync(token, cancellationToken);
        }

        public async Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
        }

        public async Task<RefreshToken?> GetByHashWithUserAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            return await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
        }

        public async Task<List<RefreshToken>> GetActiveByUserAsync(
            int userId,
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            return await _context.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > now)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<RefreshToken>> GetActiveByFamilyAsync(
            Guid familyId,
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            return await _context.RefreshTokens
                .Where(t => t.FamilyId == familyId && t.RevokedAt == null && t.ExpiresAt > now)
                .ToListAsync(cancellationToken);
        }

        public void Update(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
        }

        public async Task<int> DeleteExpiredAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default)
        {
            return await _context.RefreshTokens
                .Where(t => t.ExpiresAt <= cutoffUtc)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
