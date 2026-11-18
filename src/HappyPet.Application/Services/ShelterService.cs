using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;

namespace HappyPet.Application.Services
{
    public class ShelterService : IShelterService
    {
        private const int DefaultPageSize = 20;
        private const int MaxPageSize = 100;

        private readonly IShelterRepository _shelterRepository;
        private readonly IAdoptionRepository _adoptionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ShelterService(
            IShelterRepository shelterRepository,
            IAdoptionRepository adoptionRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _shelterRepository = shelterRepository;
            _adoptionRepository = adoptionRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ShelterProfileResponse> UpsertProfileAsync(
            int userId,
            UpdateShelterProfileRequest request,
            CancellationToken cancellationToken = default)
        {
            var organizationName = request.OrganizationName.Trim();
            var registrationNumber = TrimOrNull(request.RegistrationNumber);
            var now = DateTime.UtcNow;

            var profile = await _shelterRepository.GetByUserIdAsync(userId, cancellationToken);

            if (profile == null)
            {
                await EnsureRegistrationNumberIsAvailableAsync(
                    registrationNumber,
                    null,
                    cancellationToken);

                profile = new ShelterProfile
                {
                    UserId = userId,
                    OrganizationName = organizationName,
                    RegistrationNumber = registrationNumber,
                    Description = TrimOrNull(request.Description),
                    City = request.City.Trim(),
                    Address = TrimOrNull(request.Address),
                    PhoneNumber = request.PhoneNumber.Trim(),
                    Email = TrimOrNull(request.Email),
                    Website = TrimOrNull(request.Website),
                    IsVerified = false,
                    IsAcceptingApplications = request.IsAcceptingApplications ?? true,
                    CreatedAt = now
                };

                await _shelterRepository.AddAsync(profile, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return MapProfile(profile);
            }

            await EnsureRegistrationNumberIsAvailableAsync(
                registrationNumber,
                profile.Id,
                cancellationToken);

            // Changing the legal identity invalidates the existing admin verification.
            var legalIdentityChanged =
                !string.Equals(profile.OrganizationName, organizationName, StringComparison.Ordinal)
                || !string.Equals(profile.RegistrationNumber, registrationNumber, StringComparison.Ordinal);

            profile.OrganizationName = organizationName;
            profile.RegistrationNumber = registrationNumber;
            profile.Description = TrimOrNull(request.Description);
            profile.City = request.City.Trim();
            profile.Address = TrimOrNull(request.Address);
            profile.PhoneNumber = request.PhoneNumber.Trim();
            profile.Email = TrimOrNull(request.Email);
            profile.Website = TrimOrNull(request.Website);

            if (request.IsAcceptingApplications.HasValue)
            {
                profile.IsAcceptingApplications = request.IsAcceptingApplications.Value;
            }

            if (legalIdentityChanged && profile.IsVerified)
            {
                profile.IsVerified = false;
                profile.VerifiedAt = null;
                profile.VerifiedByUserId = null;
            }

            profile.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapProfile(profile);
        }

        public async Task<ShelterProfileResponse?> GetMyProfileAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var profile = await _shelterRepository.GetByUserIdAsync(userId, cancellationToken);
            return profile == null ? null : MapProfile(profile);
        }

        public async Task<ShelterPublicResponse> GetPublicProfileAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var profile = await _shelterRepository.GetByIdAsync(id, cancellationToken);
            if (profile == null || !profile.IsVerified)
            {
                throw new KeyNotFoundException("თავშესაფარი ვერ მოიძებნა.");
            }

            return MapPublic(profile);
        }

        public async Task<PagedResult<ShelterPublicResponse>> GetPublicListAsync(
            ShelterSearchCriteria criteria,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var (validPage, validPageSize) = NormalizePagination(page, pageSize);
            var (items, totalCount) = await _shelterRepository.GetVerifiedPagedAsync(
                criteria,
                (validPage - 1) * validPageSize,
                validPageSize,
                cancellationToken);

            return new PagedResult<ShelterPublicResponse>
            {
                Items = items.Select(MapPublic).ToList(),
                Page = validPage,
                PageSize = validPageSize,
                TotalCount = totalCount
            };
        }

        public async Task<ShelterProfileResponse> VerifyAsync(
            int id,
            int adminUserId,
            CancellationToken cancellationToken = default)
        {
            var profile = await _shelterRepository.GetByIdAsync(id, cancellationToken);
            if (profile == null)
            {
                throw new KeyNotFoundException("თავშესაფარი ვერ მოიძებნა.");
            }

            if (profile.IsVerified)
            {
                return MapProfile(profile);
            }

            var now = DateTime.UtcNow;
            profile.IsVerified = true;
            profile.VerifiedAt = now;
            profile.VerifiedByUserId = adminUserId;
            profile.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapProfile(profile);
        }

        public async Task<ShelterProfileResponse> UnverifyAsync(
            int id,
            int adminUserId,
            CancellationToken cancellationToken = default)
        {
            var profile = await _shelterRepository.GetByIdAsync(id, cancellationToken);
            if (profile == null)
            {
                throw new KeyNotFoundException("თავშესაფარი ვერ მოიძებნა.");
            }

            if (!profile.IsVerified)
            {
                return MapProfile(profile);
            }

            var now = DateTime.UtcNow;
            profile.IsVerified = false;
            profile.VerifiedAt = null;
            profile.VerifiedByUserId = adminUserId;
            profile.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapProfile(profile);
        }

        public async Task<ShelterDashboardResponse> GetDashboardAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var profile = await _shelterRepository.GetByUserIdAsync(userId, cancellationToken);
            var stats = await _adoptionRepository.GetShelterStatsAsync(userId, cancellationToken);

            return new ShelterDashboardResponse
            {
                Shelter = profile == null ? null : MapProfile(profile),
                Stats = MapStats(stats)
            };
        }

        public async Task<AdminShelterDashboardResponse> GetAdminDashboardAsync(
            CancellationToken cancellationToken = default)
        {
            var totalShelters = await _shelterRepository.CountAsync(null, cancellationToken);
            var verifiedShelters = await _shelterRepository.CountAsync(true, cancellationToken);
            var adoptionStats = await _adoptionRepository.GetAdminStatsAsync(cancellationToken);

            return new AdminShelterDashboardResponse
            {
                TotalShelters = totalShelters,
                VerifiedShelters = verifiedShelters,
                UnverifiedShelters = totalShelters - verifiedShelters,
                ActiveListings = adoptionStats.ActiveListings,
                PendingApplications = adoptionStats.PendingApplications,
                CompletedAdoptions = adoptionStats.CompletedAdoptions
            };
        }

        public async Task<PagedResult<ShelterProfileResponse>> GetAdminListAsync(
            ShelterSearchCriteria criteria,
            bool? verified = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var (validPage, validPageSize) = NormalizePagination(page, pageSize);
            var (items, totalCount) = await _shelterRepository.GetPagedAsync(
                criteria,
                verified,
                (validPage - 1) * validPageSize,
                validPageSize,
                cancellationToken);

            return new PagedResult<ShelterProfileResponse>
            {
                Items = items.Select(MapProfile).ToList(),
                Page = validPage,
                PageSize = validPageSize,
                TotalCount = totalCount
            };
        }

        public async Task GrantShelterRoleAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException("მომხმარებელი ვერ მოიძებნა.");
            }

            if (user.Role == UserRole.Shelter)
            {
                return;
            }

            if (user.Role != UserRole.User && user.Role != UserRole.PetOwner)
            {
                throw new InvalidOperationException("მოცემული როლის მქონე მომხმარებლის თავშესაფრად გადაქცევა შეუძლებელია.");
            }

            user.Role = UserRole.Shelter;
            user.UpdatedAt = DateTime.UtcNow;
            _userRepository.Update(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task EnsureRegistrationNumberIsAvailableAsync(
            string? registrationNumber,
            int? excludeProfileId,
            CancellationToken cancellationToken)
        {
            if (registrationNumber == null)
            {
                return;
            }

            if (await _shelterRepository.ExistsByRegistrationNumberAsync(
                    registrationNumber,
                    excludeProfileId,
                    cancellationToken))
            {
                throw new InvalidOperationException("მითითებული სარეგისტრაციო ნომრით თავშესაფარი უკვე რეგისტრირებულია.");
            }
        }

        private static ShelterProfileResponse MapProfile(ShelterProfile profile)
        {
            return new ShelterProfileResponse
            {
                Id = profile.Id,
                UserId = profile.UserId,
                OrganizationName = profile.OrganizationName,
                RegistrationNumber = profile.RegistrationNumber,
                Description = profile.Description,
                City = profile.City,
                Address = profile.Address,
                PhoneNumber = profile.PhoneNumber,
                Email = profile.Email,
                Website = profile.Website,
                IsVerified = profile.IsVerified,
                VerifiedAt = profile.VerifiedAt,
                VerifiedByUserId = profile.VerifiedByUserId,
                IsAcceptingApplications = profile.IsAcceptingApplications,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }

        private static ShelterPublicResponse MapPublic(ShelterProfile profile)
        {
            return new ShelterPublicResponse
            {
                Id = profile.Id,
                OrganizationName = profile.OrganizationName,
                Description = profile.Description,
                City = profile.City,
                Address = profile.Address,
                PhoneNumber = profile.PhoneNumber,
                Email = profile.Email,
                Website = profile.Website,
                IsVerified = profile.IsVerified,
                IsAcceptingApplications = profile.IsAcceptingApplications,
                CreatedAt = profile.CreatedAt
            };
        }

        private static ShelterDashboardStats MapStats(ShelterAdoptionStats stats)
        {
            return new ShelterDashboardStats
            {
                TotalListings = stats.TotalListings,
                ActiveListings = stats.ActiveListings,
                PausedListings = stats.PausedListings,
                AdoptedListings = stats.AdoptedListings,
                WithdrawnListings = stats.WithdrawnListings,
                TotalApplications = stats.TotalApplications,
                PendingApplications = stats.PendingApplications,
                ApprovedApplications = stats.ApprovedApplications,
                CompletedAdoptions = stats.CompletedAdoptions
            };
        }

        private static (int Page, int PageSize) NormalizePagination(int page, int pageSize)
        {
            var validPage = page < 1 ? 1 : page;
            var validPageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);
            return (validPage, validPageSize);
        }

        private static string? TrimOrNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
