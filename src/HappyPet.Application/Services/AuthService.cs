using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responises;
using HappyPet.Application.Exceptions;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Security;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Application.Options;
using HappyPet.Domain.Entities;
using Microsoft.Extensions.Options;

namespace HappyPet.Application.Services
{
    public class AuthService : IAuthService
    {
        private const string SessionExpiredMessage = "სესია ვადაგასულია. გთხოვთ ხელახლა შეხვიდეთ.";
        private const string SessionRevokedMessage = "სესია გაუქმებულია. გთხოვთ ხელახლა შეხვიდეთ.";

        private static readonly TimeSpan RefreshReuseGracePeriod = TimeSpan.FromSeconds(10);

        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly IRefreshTokenHasher _refreshTokenHasher;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<JwtOptions> _jwtOptions;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenGenerator tokenGenerator,
            IRefreshTokenGenerator refreshTokenGenerator,
            IRefreshTokenHasher refreshTokenHasher,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork,
            IOptions<JwtOptions> jwtOptions)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
            _refreshTokenGenerator = refreshTokenGenerator;
            _refreshTokenHasher = refreshTokenHasher;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _jwtOptions = jwtOptions;
        }

        public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                throw new UnauthorizedAccessException("არასწორი ელ-ფოსტა ან პაროლი.");
            }

            bool isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.Password);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("არასწორი ელ-ფოსტა ან პაროლი.");
            }

            if (!user.IsEmailVerified)
            {
                throw new EmailNotVerifiedException();
            }

            var now = DateTime.UtcNow;
            var accessToken = _tokenGenerator.GenerateToken(user);
            var refreshToken = await CreateRefreshTokenAsync(user.Id, Guid.NewGuid(), now, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return BuildResult(user, accessToken, refreshToken.Token, refreshToken.ExpiresAt);
        }

        public async Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new UnauthorizedAccessException(SessionExpiredMessage);
            }

            var now = DateTime.UtcNow;
            var tokenHash = _refreshTokenHasher.Hash(refreshToken.Trim());
            var storedToken = await _refreshTokenRepository.GetByHashWithUserAsync(tokenHash, cancellationToken);

            if (storedToken == null)
            {
                throw new UnauthorizedAccessException(SessionExpiredMessage);
            }

            if (storedToken.ExpiresAt <= now)
            {
                Revoke(storedToken, now, "expired");
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                throw new UnauthorizedAccessException(SessionExpiredMessage);
            }

            if (storedToken.RevokedAt != null)
            {
                // A concurrent legitimate refresh can arrive just after rotation. Reject it,
                // but do not destroy the whole family inside the short grace period.
                if (storedToken.RevokedAt.Value > now - RefreshReuseGracePeriod)
                {
                    throw new UnauthorizedAccessException(SessionExpiredMessage);
                }

                await RevokeFamilyAsync(storedToken.FamilyId, now, "reuse-detected", cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                throw new UnauthorizedAccessException(SessionRevokedMessage);
            }

            if (!storedToken.User.IsEmailVerified)
            {
                throw new EmailNotVerifiedException();
            }

            var newRawToken = _refreshTokenGenerator.GenerateToken();
            var newTokenHash = _refreshTokenHasher.Hash(newRawToken);

            storedToken.RevokedAt = now;
            storedToken.RevocationReason = "rotated";
            storedToken.ReplacedByTokenHash = newTokenHash;
            storedToken.UpdatedAt = now;

            var newToken = CreateRefreshTokenEntity(
                storedToken.UserId,
                storedToken.FamilyId,
                newTokenHash,
                now);

            _refreshTokenRepository.Update(storedToken);
            await _refreshTokenRepository.AddAsync(newToken, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var accessToken = _tokenGenerator.GenerateToken(storedToken.User);

            return BuildResult(storedToken.User, accessToken, newRawToken, newToken.ExpiresAt);
        }

        public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }

            var now = DateTime.UtcNow;
            var tokenHash = _refreshTokenHasher.Hash(refreshToken.Trim());
            var storedToken = await _refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);

            if (storedToken == null || storedToken.RevokedAt != null)
            {
                return;
            }

            Revoke(storedToken, now, "logout");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task LogoutAllAsync(int userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var activeTokens = await _refreshTokenRepository.GetActiveByUserAsync(userId, now, cancellationToken);

            if (activeTokens.Count == 0)
            {
                return;
            }

            foreach (var token in activeTokens)
            {
                Revoke(token, now, "logout-all");
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<(string Token, DateTime ExpiresAt)> CreateRefreshTokenAsync(
            int userId,
            Guid familyId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var rawToken = _refreshTokenGenerator.GenerateToken();
            var tokenHash = _refreshTokenHasher.Hash(rawToken);
            var token = CreateRefreshTokenEntity(userId, familyId, tokenHash, now);

            await _refreshTokenRepository.AddAsync(token, cancellationToken);

            return (rawToken, token.ExpiresAt);
        }

        private RefreshToken CreateRefreshTokenEntity(int userId, Guid familyId, string tokenHash, DateTime now)
        {
            var lifetimeDays = _jwtOptions.Value.RefreshTokenDays;
            if (lifetimeDays <= 0)
            {
                throw new InvalidOperationException("Jwt:RefreshTokenDays უნდა იყოს დადებითი რიცხვი.");
            }

            return new RefreshToken
            {
                UserId = userId,
                FamilyId = familyId,
                TokenHash = tokenHash,
                ExpiresAt = now.AddDays(lifetimeDays),
                CreatedAt = now
            };
        }

        private async Task RevokeFamilyAsync(
            Guid familyId,
            DateTime now,
            string reason,
            CancellationToken cancellationToken)
        {
            var activeTokens = await _refreshTokenRepository.GetActiveByFamilyAsync(familyId, now, cancellationToken);

            foreach (var token in activeTokens)
            {
                Revoke(token, now, reason);
            }
        }

        private void Revoke(RefreshToken token, DateTime now, string reason)
        {
            token.RevokedAt = now;
            token.RevocationReason = reason;
            token.UpdatedAt = now;
            _refreshTokenRepository.Update(token);
        }

        private static AuthResult BuildResult(
            User user,
            AccessTokenResult accessToken,
            string refreshToken,
            DateTime refreshTokenExpiresAt)
        {
            return new AuthResult
            {
                Response = new LoginResponse
                {
                    AccessToken = accessToken.Token,
                    ExpiresIn = accessToken.ExpiresIn,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role.ToString(),
                    IsEmailVerified = user.IsEmailVerified
                },
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            };
        }
    }
}
