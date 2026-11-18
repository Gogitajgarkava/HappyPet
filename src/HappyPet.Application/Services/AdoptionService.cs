using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Application.Interfaces.Storage;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;

namespace HappyPet.Application.Services
{
    public class AdoptionService : IAdoptionService
    {
        private const int DefaultPageSize = 20;
        private const int MaxPageSize = 100;
        private const string OtherApplicationSucceededNote = "სხვა განაცხადი დაკმაყოფილდა.";

        private readonly IAdoptionRepository _adoptionRepository;
        private readonly IShelterRepository _shelterRepository;
        private readonly IPetRepository _petRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorage;

        public AdoptionService(
            IAdoptionRepository adoptionRepository,
            IShelterRepository shelterRepository,
            IPetRepository petRepository,
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorage)
        {
            _adoptionRepository = adoptionRepository;
            _shelterRepository = shelterRepository;
            _petRepository = petRepository;
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
        }

        public async Task<AdoptionListingResponse> CreateListingAsync(
            int shelterUserId,
            CreateAdoptionListingRequest request,
            CancellationToken cancellationToken = default)
        {
            var shelter = await GetVerifiedShelterAsync(shelterUserId, cancellationToken);

            var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
            if (pet == null)
            {
                throw new KeyNotFoundException("ცხოველი ვერ მოიძებნა.");
            }

            if (pet.OwnerId != shelterUserId)
            {
                throw new InvalidOperationException("მითითებული ცხოველი თავშესაფარს არ ეკუთვნის.");
            }

            if (pet.IsDeceased)
            {
                throw new InvalidOperationException("გარდაცვლილი ცხოველის განსაცხოვრებლად განთავსება შეუძლებელია.");
            }

            if (await _adoptionRepository.HasActiveListingForPetAsync(
                    pet.Id,
                    null,
                    cancellationToken))
            {
                throw new InvalidOperationException("ამ ცხოველისთვის უკვე არსებობს აქტიური განცხადება.");
            }

            var now = DateTime.UtcNow;
            var listing = new AdoptionListing
            {
                ShelterId = shelterUserId,
                PetId = pet.Id,
                Description = TrimOrNull(request.Description),
                City = request.City.Trim(),
                AdoptionFee = request.AdoptionFee,
                IsUrgent = request.IsUrgent,
                Status = AdoptionListingStatus.Active,
                PublishedAt = now,
                CreatedAt = now,
                Pet = pet,
                Shelter = shelter.User
            };

            await _adoptionRepository.AddListingAsync(listing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapListing(listing);
        }

        public async Task<AdoptionListingResponse?> GetListingAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var listing = await _adoptionRepository.GetListingByIdAsync(id, cancellationToken);
            return listing == null ? null : MapListing(listing);
        }

        public async Task<AdoptionListingResponse?> GetPublicListingAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var listing = await _adoptionRepository.GetListingByIdAsync(id, cancellationToken);
            if (listing == null
                || (listing.Status != AdoptionListingStatus.Active
                    && listing.Status != AdoptionListingStatus.Adopted)
                || listing.Shelter.ShelterProfile == null
                || !listing.Shelter.ShelterProfile.IsVerified)
            {
                return null;
            }

            return MapListing(listing);
        }

        public async Task<PagedResult<AdoptionListingResponse>> GetActiveListingsAsync(
            AdoptionSearchCriteria criteria,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var (validPage, validPageSize) = NormalizePagination(page, pageSize);
            var (items, totalCount) = await _adoptionRepository.GetActiveListingsPagedAsync(
                criteria,
                (validPage - 1) * validPageSize,
                validPageSize,
                cancellationToken);

            return new PagedResult<AdoptionListingResponse>
            {
                Items = items.Select(MapListing).ToList(),
                Page = validPage,
                PageSize = validPageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PagedResult<AdoptionListingResponse>> GetShelterListingsAsync(
            int shelterUserId,
            AdoptionListingStatus? status = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var (validPage, validPageSize) = NormalizePagination(page, pageSize);
            var (items, totalCount) = await _adoptionRepository.GetListingsByShelterPagedAsync(
                shelterUserId,
                status,
                (validPage - 1) * validPageSize,
                validPageSize,
                cancellationToken);

            return new PagedResult<AdoptionListingResponse>
            {
                Items = items.Select(MapListing).ToList(),
                Page = validPage,
                PageSize = validPageSize,
                TotalCount = totalCount
            };
        }

        public async Task<AdoptionListingResponse> UpdateListingAsync(
            int id,
            int shelterUserId,
            UpdateAdoptionListingRequest request,
            CancellationToken cancellationToken = default)
        {
            var listing = await GetManagedListingAsync(id, shelterUserId, cancellationToken);

            if (listing.Status != AdoptionListingStatus.Active
                && listing.Status != AdoptionListingStatus.Paused)
            {
                throw new InvalidOperationException("მხოლოდ აქტიური ან შეჩერებული განცხადების რედაქტირებაა შესაძლებელი.");
            }

            listing.Description = TrimOrNull(request.Description);
            listing.City = request.City.Trim();
            listing.AdoptionFee = request.AdoptionFee;
            listing.IsUrgent = request.IsUrgent;
            listing.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapListing(listing);
        }

        public async Task<AdoptionListingResponse> WithdrawListingAsync(
            int id,
            int shelterUserId,
            CancellationToken cancellationToken = default)
        {
            var listing = await GetManagedListingAsync(id, shelterUserId, cancellationToken);

            if (listing.Status == AdoptionListingStatus.Withdrawn)
            {
                return MapListing(listing);
            }

            if (listing.Status != AdoptionListingStatus.Active
                && listing.Status != AdoptionListingStatus.Paused)
            {
                throw new InvalidOperationException("მხოლოდ აქტიური ან შეჩერებული განცხადების გაუქმებაა შესაძლებელი.");
            }

            var now = DateTime.UtcNow;
            listing.Status = AdoptionListingStatus.Withdrawn;
            listing.ClosedAt = now;
            listing.UpdatedAt = now;

            var applications = await _adoptionRepository.GetApplicationsByListingAsync(id, cancellationToken);
            foreach (var application in applications.Where(IsOpenApplication))
            {
                application.Status = AdoptionApplicationStatus.Rejected;
                application.ReviewedAt = now;
                application.ReviewedByUserId = shelterUserId;
                application.DecisionNote = "განცხადება გაუქმდა თავშესაფრის მიერ.";
                application.UpdatedAt = now;
                _adoptionRepository.UpdateApplication(application);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapListing(listing);
        }

        public async Task<AdoptionListingResponse> PauseListingAsync(
            int id,
            int shelterUserId,
            CancellationToken cancellationToken = default)
        {
            var listing = await GetManagedListingAsync(id, shelterUserId, cancellationToken);

            if (listing.Status == AdoptionListingStatus.Paused)
            {
                return MapListing(listing);
            }

            if (listing.Status != AdoptionListingStatus.Active)
            {
                throw new InvalidOperationException("მხოლოდ აქტიური განცხადების შეჩერებაა შესაძლებელი.");
            }

            listing.Status = AdoptionListingStatus.Paused;
            listing.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapListing(listing);
        }

        public async Task<AdoptionListingResponse> ResumeListingAsync(
            int id,
            int shelterUserId,
            CancellationToken cancellationToken = default)
        {
            var listing = await GetManagedListingAsync(id, shelterUserId, cancellationToken);

            if (listing.Status == AdoptionListingStatus.Active)
            {
                return MapListing(listing);
            }

            if (listing.Status != AdoptionListingStatus.Paused)
            {
                throw new InvalidOperationException("მხოლოდ შეჩერებული განცხადების განახლებაა შესაძლებელი.");
            }

            if (listing.Shelter.ShelterProfile == null || !listing.Shelter.ShelterProfile.IsVerified)
            {
                throw new InvalidOperationException("განცხადების განახლებამდე საჭიროა თავშესაფრის ვერიფიკაცია.");
            }

            listing.Status = AdoptionListingStatus.Active;
            listing.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapListing(listing);
        }

        public async Task<AdoptionApplicationResponse> ApplyAsync(
            int listingId,
            int applicantUserId,
            CreateAdoptionApplicationRequest request,
            CancellationToken cancellationToken = default)
        {
            var listing = await _adoptionRepository.GetListingByIdAsync(listingId, cancellationToken);
            if (listing == null)
            {
                throw new KeyNotFoundException("განცხადება ვერ მოიძებნა.");
            }

            if (listing.Status != AdoptionListingStatus.Active)
            {
                throw new InvalidOperationException("ამ განცხადებაზე მიმდინარე განაცხადების მიღება შეუძლებელია.");
            }

            if (listing.Shelter.ShelterProfile == null || !listing.Shelter.ShelterProfile.IsVerified)
            {
                throw new InvalidOperationException("თავშესაფარი ვერიფიცირებული არ არის.");
            }

            if (!listing.Shelter.ShelterProfile.IsAcceptingApplications)
            {
                throw new InvalidOperationException("თავშესაფარი ამჟამად არ იღებს განაცხადებებს.");
            }

            if (applicantUserId == listing.ShelterId)
            {
                throw new InvalidOperationException("საკუთარ განცხადებაზე განაცხადის გაგზავნა შეუძლებელია.");
            }

            if (await _adoptionRepository.GetApplicationByListingAndApplicantAsync(
                    listingId,
                    applicantUserId,
                    cancellationToken) != null)
            {
                throw new InvalidOperationException("ამ განცხადებაზე უკვე გაქვთ განაცხადი.");
            }

            var now = DateTime.UtcNow;
            var application = new AdoptionApplication
            {
                ListingId = listingId,
                ApplicantUserId = applicantUserId,
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                Phone = request.Phone.Trim(),
                Address = TrimOrNull(request.Address),
                Message = TrimOrNull(request.Message),
                Status = AdoptionApplicationStatus.Pending,
                CreatedAt = now,
                Listing = listing
            };

            await _adoptionRepository.AddApplicationAsync(application, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapApplication(application);
        }

        public async Task<AdoptionApplicationResponse?> GetApplicationAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var application = await _adoptionRepository.GetApplicationByIdAsync(id, cancellationToken);
            return application == null ? null : MapApplication(application);
        }

        public async Task<PagedResult<AdoptionApplicationResponse>> GetListingApplicationsAsync(
            int listingId,
            int shelterUserId,
            AdoptionApplicationStatus? status = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var listing = await _adoptionRepository.GetListingByIdAsync(listingId, cancellationToken);
            if (listing == null)
            {
                throw new KeyNotFoundException("განცხადება ვერ მოიძებნა.");
            }

            EnsureShelterOwnsListing(listing, shelterUserId);

            var (validPage, validPageSize) = NormalizePagination(page, pageSize);
            var (items, totalCount) = await _adoptionRepository.GetApplicationsByListingPagedAsync(
                listingId,
                status,
                (validPage - 1) * validPageSize,
                validPageSize,
                cancellationToken);

            return new PagedResult<AdoptionApplicationResponse>
            {
                Items = items.Select(MapApplication).ToList(),
                Page = validPage,
                PageSize = validPageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PagedResult<AdoptionApplicationResponse>> GetMyApplicationsAsync(
            int applicantUserId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var (validPage, validPageSize) = NormalizePagination(page, pageSize);
            var (items, totalCount) = await _adoptionRepository.GetApplicationsByApplicantPagedAsync(
                applicantUserId,
                (validPage - 1) * validPageSize,
                validPageSize,
                cancellationToken);

            return new PagedResult<AdoptionApplicationResponse>
            {
                Items = items.Select(MapApplication).ToList(),
                Page = validPage,
                PageSize = validPageSize,
                TotalCount = totalCount
            };
        }

        public async Task<AdoptionApplicationResponse> ApproveApplicationAsync(
            int applicationId,
            int shelterUserId,
            ReviewAdoptionApplicationRequest request,
            CancellationToken cancellationToken = default)
        {
            var application = await GetManagedApplicationAsync(applicationId, shelterUserId, cancellationToken);

            if (application.Status != AdoptionApplicationStatus.Pending)
            {
                throw new InvalidOperationException("მხოლოდ მოლოდინში მყოფი განაცხადის დამტკიცებაა შესაძლებელი.");
            }

            var now = DateTime.UtcNow;
            application.Status = AdoptionApplicationStatus.Approved;
            application.ReviewedAt = now;
            application.ReviewedByUserId = shelterUserId;
            application.DecisionNote = TrimOrNull(request.Note);
            application.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapApplication(application);
        }

        public async Task<AdoptionApplicationResponse> RejectApplicationAsync(
            int applicationId,
            int shelterUserId,
            ReviewAdoptionApplicationRequest request,
            CancellationToken cancellationToken = default)
        {
            var application = await GetManagedApplicationAsync(applicationId, shelterUserId, cancellationToken);

            if (application.Status != AdoptionApplicationStatus.Pending
                && application.Status != AdoptionApplicationStatus.Approved)
            {
                throw new InvalidOperationException("მხოლოდ მოლოდინში ან დამტკიცებული განაცხადის უარყოფაა შესაძლებელი.");
            }

            var now = DateTime.UtcNow;
            application.Status = AdoptionApplicationStatus.Rejected;
            application.ReviewedAt = now;
            application.ReviewedByUserId = shelterUserId;
            application.DecisionNote = TrimOrNull(request.Note);
            application.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapApplication(application);
        }

        public async Task<AdoptionApplicationResponse> CompleteApplicationAsync(
            int applicationId,
            int shelterUserId,
            ReviewAdoptionApplicationRequest request,
            CancellationToken cancellationToken = default)
        {
            var application = await GetManagedApplicationAsync(applicationId, shelterUserId, cancellationToken);

            if (application.Status != AdoptionApplicationStatus.Approved)
            {
                throw new InvalidOperationException("დასრულებამდე განაცხადი უნდა იყოს დამტკიცებული.");
            }

            var listing = application.Listing;
            if (listing.Status != AdoptionListingStatus.Active
                && listing.Status != AdoptionListingStatus.Paused)
            {
                throw new InvalidOperationException("მხოლოდ აქტიური ან შეჩერებული განცხადების დასრულებაა შესაძლებელი.");
            }

            var pet = await _petRepository.GetByIdAsync(listing.PetId, cancellationToken);
            if (pet == null)
            {
                throw new InvalidOperationException("დაკავშირებული ცხოველი ვერ მოიძებნა.");
            }

            if (pet.IsDeceased)
            {
                throw new InvalidOperationException("გარდაცვლილი ცხოველის გადაცემა შეუძლებელია.");
            }

            if (pet.OwnerId != listing.ShelterId)
            {
                throw new InvalidOperationException("ცხოველის მფლობელი შეიცვალა და განაცხადის დასრულება შეუძლებელია.");
            }

            var now = DateTime.UtcNow;

            pet.OwnerId = application.ApplicantUserId;
            pet.UpdatedAt = now;
            _petRepository.Update(pet);

            application.Status = AdoptionApplicationStatus.Completed;
            application.ReviewedAt = now;
            application.ReviewedByUserId = shelterUserId;
            application.DecisionNote = TrimOrNull(request.Note);
            application.UpdatedAt = now;

            listing.Status = AdoptionListingStatus.Adopted;
            listing.ClosedAt = now;
            listing.UpdatedAt = now;

            var applications = await _adoptionRepository.GetApplicationsByListingAsync(
                listing.Id,
                cancellationToken);

            foreach (var other in applications.Where(
                         a => a.Id != application.Id && IsOpenApplication(a)))
            {
                other.Status = AdoptionApplicationStatus.Rejected;
                other.ReviewedAt = now;
                other.ReviewedByUserId = shelterUserId;
                other.DecisionNote = OtherApplicationSucceededNote;
                other.UpdatedAt = now;
                _adoptionRepository.UpdateApplication(other);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapApplication(application);
        }

        public async Task<AdoptionApplicationResponse> WithdrawApplicationAsync(
            int applicationId,
            int applicantUserId,
            CancellationToken cancellationToken = default)
        {
            var application = await _adoptionRepository.GetApplicationByIdAsync(
                applicationId,
                cancellationToken);

            if (application == null)
            {
                throw new KeyNotFoundException("განაცხადი ვერ მოიძებნა.");
            }

            if (application.ApplicantUserId != applicantUserId)
            {
                throw new UnauthorizedAccessException("განაცხადი სხვა მომხმარებელს ეკუთვნის.");
            }

            if (application.Status == AdoptionApplicationStatus.Withdrawn)
            {
                return MapApplication(application);
            }

            if (application.Status != AdoptionApplicationStatus.Pending
                && application.Status != AdoptionApplicationStatus.Approved)
            {
                throw new InvalidOperationException("ამ სტატუსის მქონე განაცხადის გაუქმება შეუძლებელია.");
            }

            var now = DateTime.UtcNow;
            application.Status = AdoptionApplicationStatus.Withdrawn;
            application.WithdrawnAt = now;
            application.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapApplication(application);
        }

        private async Task<ShelterProfile> GetVerifiedShelterAsync(
            int shelterUserId,
            CancellationToken cancellationToken)
        {
            var shelter = await _shelterRepository.GetByUserIdAsync(shelterUserId, cancellationToken);
            if (shelter == null)
            {
                throw new InvalidOperationException("განცხადების გამოსაქვეყნებლად ჯერ შეავსეთ თავშესაფრის პროფილი.");
            }

            if (!shelter.IsVerified)
            {
                throw new InvalidOperationException("განცხადების გამოქვეყნებამდე საჭიროა ადმინისტრატორის ვერიფიკაცია.");
            }

            return shelter;
        }

        private async Task<AdoptionListing> GetManagedListingAsync(
            int id,
            int shelterUserId,
            CancellationToken cancellationToken)
        {
            var listing = await _adoptionRepository.GetListingByIdAsync(id, cancellationToken);
            if (listing == null)
            {
                throw new KeyNotFoundException("განცხადება ვერ მოიძებნა.");
            }

            EnsureShelterOwnsListing(listing, shelterUserId);
            return listing;
        }

        private async Task<AdoptionApplication> GetManagedApplicationAsync(
            int id,
            int shelterUserId,
            CancellationToken cancellationToken)
        {
            var application = await _adoptionRepository.GetApplicationByIdAsync(id, cancellationToken);
            if (application == null)
            {
                throw new KeyNotFoundException("განაცხადი ვერ მოიძებნა.");
            }

            EnsureShelterOwnsListing(application.Listing, shelterUserId);
            return application;
        }

        private static void EnsureShelterOwnsListing(AdoptionListing listing, int shelterUserId)
        {
            if (listing.ShelterId != shelterUserId)
            {
                throw new UnauthorizedAccessException("განცხადება სხვა თავშესაფარს ეკუთვნის.");
            }
        }

        private static bool IsOpenApplication(AdoptionApplication application)
        {
            return application.Status == AdoptionApplicationStatus.Pending
                   || application.Status == AdoptionApplicationStatus.Approved;
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

        private AdoptionListingResponse MapListing(AdoptionListing listing)
        {
            var pet = listing.Pet;
            var profile = listing.Shelter?.ShelterProfile;
            var shelterName = profile?.OrganizationName
                ?? (listing.Shelter == null
                    ? string.Empty
                    : $"{listing.Shelter.FirstName} {listing.Shelter.LastName}".Trim());

            var ageMonths = pet?.BirthDate.HasValue == true
                ? Math.Max(0, (int)((DateTime.UtcNow.Date - pet.BirthDate!.Value.Date).TotalDays / 30.44))
                : (int?)null;

            return new AdoptionListingResponse
            {
                Id = listing.Id,
                ShelterId = listing.ShelterId,
                Shelter = new ShelterSummaryResponse
                {
                    Id = profile?.Id ?? 0,
                    OrganizationName = shelterName,
                    City = profile?.City ?? listing.City,
                    PhoneNumber = profile?.PhoneNumber ?? listing.Shelter?.PhoneNumber ?? string.Empty,
                    Email = profile?.Email,
                    Website = profile?.Website,
                    IsVerified = profile?.IsVerified ?? false,
                    IsAcceptingApplications = profile?.IsAcceptingApplications ?? false
                },
                PetId = listing.PetId,
                PetName = pet?.Name ?? string.Empty,
                Species = pet?.Species ?? PetSpecies.Other,
                Breed = pet?.Breed,
                Gender = pet?.Gender ?? PetGender.Male,
                BirthDate = pet?.BirthDate,
                AgeMonths = ageMonths,
                Color = pet?.Color,
                Size = pet?.Size,
                IsSterilized = pet?.IsSterilized ?? false,
                Description = listing.Description,
                City = listing.City,
                AdoptionFee = listing.AdoptionFee,
                IsUrgent = listing.IsUrgent,
                Status = listing.Status,
                PublishedAt = listing.PublishedAt,
                ClosedAt = listing.ClosedAt,
                CreatedAt = listing.CreatedAt,
                UpdatedAt = listing.UpdatedAt,
                Photos = pet?.Photos
                    .OrderByDescending(p => p.IsPrimary)
                    .ThenBy(p => p.CreatedAt)
                    .Select(MapPhoto)
                    .ToList() ?? new List<PetPhotoResponse>()
            };
        }

        private AdoptionApplicationResponse MapApplication(AdoptionApplication application)
        {
            var listing = application.Listing;
            var applicant = application.Applicant;
            var profile = listing?.Shelter?.ShelterProfile;
            var applicantFullName = applicant == null
                ? application.FullName
                : $"{applicant.FirstName} {applicant.LastName}".Trim();

            return new AdoptionApplicationResponse
            {
                Id = application.Id,
                ListingId = application.ListingId,
                ShelterId = listing?.ShelterId ?? 0,
                ShelterName = profile?.OrganizationName
                    ?? (listing?.Shelter == null
                        ? string.Empty
                        : $"{listing.Shelter.FirstName} {listing.Shelter.LastName}".Trim()),
                PetId = listing?.PetId ?? 0,
                PetName = listing?.Pet?.Name ?? string.Empty,
                ApplicantUserId = application.ApplicantUserId,
                ApplicantFullName = applicantFullName,
                FullName = application.FullName,
                Email = application.Email,
                Phone = application.Phone,
                Address = application.Address,
                Message = application.Message,
                Status = application.Status,
                ReviewedAt = application.ReviewedAt,
                ReviewedByUserId = application.ReviewedByUserId,
                DecisionNote = application.DecisionNote,
                WithdrawnAt = application.WithdrawnAt,
                CreatedAt = application.CreatedAt,
                UpdatedAt = application.UpdatedAt
            };
        }

        private PetPhotoResponse MapPhoto(PetPhoto photo)
        {
            return new PetPhotoResponse
            {
                Id = photo.Id,
                PetId = photo.PetId,
                Url = _fileStorage.GetUrl(photo.Key),
                IsPrimary = photo.IsPrimary,
                CreatedAt = photo.CreatedAt
            };
        }
    }
}
