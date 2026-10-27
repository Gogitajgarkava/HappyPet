using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyPet.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly HappyPetDbContext _context;

        public UserRepository(HappyPetDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
        }

        public void Delete(User user)
        {
            _context.Users.Remove(user);
        }

        public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var normalized = email.Trim().ToLowerInvariant();
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized, cancellationToken);
        }

        public async Task<User?> GetByPersonalIdAsync(string personalId, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.PersonalId == personalId, cancellationToken);
        }

        public async Task<User?> GetWithPetsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.Pets)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var normalized = email.Trim().ToLowerInvariant();
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == normalized, cancellationToken);
        }

        public async Task<bool> ExistsByPersonalIdAsync(string personalId, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AnyAsync(u => u.PersonalId == personalId, cancellationToken);
        }

        public async Task AddOtpCodeAsync(UserOtpCode code, CancellationToken cancellationToken = default)
        {
            await _context.UserOtpCodes.AddAsync(code, cancellationToken);
        }

        public async Task<UserOtpCode?> GetLatestOtpCodeAsync(
            int userId,
            OtpPurpose purpose,
            CancellationToken cancellationToken = default)
        {
            return await _context.UserOtpCodes
                .Where(c => c.UserId == userId && c.Purpose == purpose)
                .OrderByDescending(c => c.CreatedAt)
                .ThenByDescending(c => c.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<UserOtpCode>> GetActiveOtpCodesAsync(
            int userId,
            OtpPurpose purpose,
            CancellationToken cancellationToken = default)
        {
            return await _context.UserOtpCodes
                .Where(c => c.UserId == userId && c.Purpose == purpose && c.ConsumedAt == null)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountOtpCodesSinceAsync(
            int userId,
            OtpPurpose purpose,
            DateTime sinceUtc,
            CancellationToken cancellationToken = default)
        {
            return await _context.UserOtpCodes
                .CountAsync(
                    c => c.UserId == userId && c.Purpose == purpose && c.CreatedAt >= sinceUtc,
                    cancellationToken);
        }

        public void UpdateOtpCode(UserOtpCode code)
        {
            _context.UserOtpCodes.Update(code);
        }
    }
}
