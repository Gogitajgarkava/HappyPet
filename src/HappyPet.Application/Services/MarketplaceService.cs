using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Application.Interfaces.Storage;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;

namespace HappyPet.Application.Services
{
    public class MarketplaceService : IMarketplaceService
    {
        private const int ListingLifetimeDays = 30;
        private const int MinAgeDays = 56;
        private const int MaxPhotosPerListing = 5;
        private const long MaxPhotoSizeBytes = 5 * 1024 * 1024;
        private const int DefaultPageSize = 20;
        private const int MaxPageSize = 100;

        private static readonly Dictionary<string, string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp"
        };

        private readonly IMarketplaceRepository _marketplaceRepository;
        private readonly IPetRepository _petRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorage;

        public MarketplaceService(
            IMarketplaceRepository marketplaceRepository,
            IPetRepository petRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorage)
        {
            _marketplaceRepository = marketplaceRepository;
            _petRepository = petRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
        }

        public async Task<MarketplaceListingResponse> CreateAsync(
            int sellerId,
            CreateMarketplaceListingRequest request,
            CancellationToken cancellationToken = default)
        {
            EnsureBusinessRules(request.ListingType, request.Price);
            EnsureMinimumAge(request.BirthDate);

            var now = DateTime.UtcNow;
            var birthDate = NormalizeDate(request.BirthDate);
            var microchipNumber = NormalizeMicrochip(request.MicrochipNumber);

            if (request.PetId.HasValue)
            {
                microchipNumber = await ValidateLinkedPetAsync(
                    request.PetId.Value,
                    sellerId,
                    microchipNumber,
                    null,
                    cancellationToken);
            }

            var listing = new MarketplaceListing
            {
                SellerId = sellerId,
                PetId = request.PetId,
                PetName = request.PetName.Trim(),
                Species = request.Species,
                Breed = TrimOrNull(request.Breed),
                Gender = request.Gender,
                BirthDate = birthDate,
                Color = TrimOrNull(request.Color),
                Size = request.Size,
                IsSterilized = request.IsSterilized,
                IsVaccinated = request.IsVaccinated,
                MicrochipNumber = microchipNumber,
                ListingType = request.ListingType,
                Price = NormalizePrice(request.ListingType, request.Price),
                City = request.City.Trim(),
                ContactPhone = request.ContactPhone.Trim(),
                Description = TrimOrNull(request.Description),
                Status = ListingStatus.Active,
                ExpiresAt = now.AddDays(ListingLifetimeDays),
                CreatedAt = now
            };

            await _marketplaceRepository.AddAsync(listing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToListing(listing);
        }

        public async Task<MarketplaceListingResponse?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var listing = await _marketplaceRepository.GetByIdAsync(id, cancellationToken);
            return listing == null ? null : MapToListing(listing);
        }

        public async Task<PagedResult<MarketplaceListingResponse>> GetActiveAsync(
            MarketplaceSearchCriteria criteria,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var (validPage, validPageSize) = NormalizePagination(page, pageSize);
            var (items, totalCount) = await _marketplaceRepository.GetActivePagedAsync(
                criteria,
                DateTime.UtcNow,
                (validPage - 1) * validPageSize,
                validPageSize,
                cancellationToken);

            return ToPagedResult(items, validPage, validPageSize, totalCount);
        }

        public async Task<PagedResult<MarketplaceListingResponse>> GetBySellerAsync(
            int sellerId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var (validPage, validPageSize) = NormalizePagination(page, pageSize);
            var (items, totalCount) = await _marketplaceRepository.GetBySellerPagedAsync(
                sellerId,
                (validPage - 1) * validPageSize,
                validPageSize,
                cancellationToken);

            return ToPagedResult(items, validPage, validPageSize, totalCount);
        }

        public async Task<MarketplaceListingResponse> UpdateAsync(
            int id,
            UpdateMarketplaceListingRequest request,
            CancellationToken cancellationToken = default)
        {
            var listing = await GetEditableListingAsync(id, cancellationToken);

            EnsureBusinessRules(request.ListingType, request.Price);
            EnsureMinimumAge(request.BirthDate);

            var birthDate = NormalizeDate(request.BirthDate);
            var microchipNumber = NormalizeMicrochip(request.MicrochipNumber);

            if (request.PetId.HasValue)
            {
                microchipNumber = await ValidateLinkedPetAsync(
                    request.PetId.Value,
                    listing.SellerId,
                    microchipNumber,
                    listing.Id,
                    cancellationToken);
            }

            listing.PetId = request.PetId;
            listing.PetName = request.PetName.Trim();
            listing.Species = request.Species;
            listing.Breed = TrimOrNull(request.Breed);
            listing.Gender = request.Gender;
            listing.BirthDate = birthDate;
            listing.Color = TrimOrNull(request.Color);
            listing.Size = request.Size;
            listing.IsSterilized = request.IsSterilized;
            listing.IsVaccinated = request.IsVaccinated;
            listing.MicrochipNumber = microchipNumber;
            listing.ListingType = request.ListingType;
            listing.Price = NormalizePrice(request.ListingType, request.Price);
            listing.City = request.City.Trim();
            listing.ContactPhone = request.ContactPhone.Trim();
            listing.Description = TrimOrNull(request.Description);
            listing.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var photos = await _marketplaceRepository.GetPhotosByListingIdAsync(id, cancellationToken);
            return MapToListing(listing, photos);
        }

        public async Task WithdrawAsync(int id, CancellationToken cancellationToken = default)
        {
            var listing = await _marketplaceRepository.GetByIdAsync(id, cancellationToken);
            if (listing == null)
            {
                throw new KeyNotFoundException("განცხადება ვერ მოიძებნა.");
            }

            if (listing.Status == ListingStatus.Withdrawn)
            {
                return;
            }

            if (listing.Status != ListingStatus.Active)
            {
                throw new InvalidOperationException("მხოლოდ აქტიური განცხადების გაუქმებაა შესაძლებელი.");
            }

            var now = DateTime.UtcNow;
            if (listing.ExpiresAt <= now)
            {
                listing.Status = ListingStatus.Expired;
                listing.UpdatedAt = now;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                throw new InvalidOperationException("განცხადების ვადა ამოიწურა.");
            }

            listing.Status = ListingStatus.Withdrawn;
            listing.WithdrawnAt = now;
            listing.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<CompleteMarketplaceListingResponse> CompleteAsync(
            int id,
            int completedByUserId,
            CompleteMarketplaceListingRequest request,
            CancellationToken cancellationToken = default)
        {
            var listing = await _marketplaceRepository.GetByIdAsync(id, cancellationToken);
            if (listing == null)
            {
                throw new KeyNotFoundException("განცხადება ვერ მოიძებნა.");
            }

            if (listing.Status != ListingStatus.Active)
            {
                throw new InvalidOperationException("დასრულება შესაძლებელია მხოლოდ აქტიური განცხადებისთვის.");
            }

            var now = DateTime.UtcNow;
            if (listing.ExpiresAt <= now)
            {
                listing.Status = ListingStatus.Expired;
                listing.UpdatedAt = now;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                throw new InvalidOperationException("განცხადების ვადა ამოიწურა.");
            }

            User? buyer = null;
            if (request.BuyerUserId.HasValue)
            {
                if (request.BuyerUserId.Value == listing.SellerId)
                {
                    throw new InvalidOperationException("გამყიდველი არ შეიძლება იყოს მყიდველი.");
                }

                buyer = await _userRepository.GetByIdAsync(request.BuyerUserId.Value, cancellationToken);
                if (buyer == null)
                {
                    throw new KeyNotFoundException("მყიდველი ვერ მოიძებნა.");
                }
            }

            string? microchipNumber = NormalizeMicrochip(listing.MicrochipNumber);
            if (listing.PetId.HasValue)
            {
                var pet = await _petRepository.GetByIdAsync(listing.PetId.Value, cancellationToken);
                if (pet == null)
                {
                    throw new InvalidOperationException("დაკავშირებული ცხოველი ვერ მოიძებნა.");
                }

                if (pet.IsDeceased)
                {
                    throw new InvalidOperationException("გარდაცვლილი ცხოველის გადაცემა შეუძლებელია.");
                }

                if (pet.OwnerId != listing.SellerId)
                {
                    throw new InvalidOperationException("ცხოველის მფლობელი შეიცვალა და განცხადების დასრულება შეუძლებელია.");
                }

                microchipNumber ??= NormalizeMicrochip(pet.MicrochipNumber);
            }

            var transferredAt = request.TransferredAt.HasValue
                ? ToUtc(request.TransferredAt.Value)
                : now;

            var transfer = new OwnershipTransfer
            {
                ListingId = listing.Id,
                PetId = listing.PetId,
                MicrochipNumber = microchipNumber,
                SellerId = listing.SellerId,
                BuyerUserId = request.BuyerUserId,
                BuyerFullName = request.BuyerFullName.Trim(),
                BuyerPersonalId = request.BuyerPersonalId.Trim(),
                BuyerPhone = TrimOrNull(request.BuyerPhone) ?? buyer?.PhoneNumber,
                TransferType = listing.ListingType == ListingType.Sale
                    ? OwnershipTransferType.Sale
                    : OwnershipTransferType.Gift,
                Price = listing.ListingType == ListingType.Sale ? listing.Price : null,
                Status = OwnershipTransferStatus.PendingAirsSync,
                TransferredAt = transferredAt,
                Notes = TrimOrNull(request.Notes),
                CreatedAt = now
            };

            listing.Status = ListingStatus.Completed;
            listing.CompletedAt = now;
            listing.CompletedByUserId = completedByUserId;
            listing.UpdatedAt = now;

            await _marketplaceRepository.AddOwnershipTransferAsync(transfer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var photos = await _marketplaceRepository.GetPhotosByListingIdAsync(id, cancellationToken);
            return new CompleteMarketplaceListingResponse
            {
                Listing = MapToListing(listing, photos),
                OwnershipTransfer = MapTransfer(transfer)
            };
        }

        public async Task<MarketplaceListingResponse> BlockAsync(
            int id,
            int adminUserId,
            BlockMarketplaceListingRequest request,
            CancellationToken cancellationToken = default)
        {
            var listing = await _marketplaceRepository.GetByIdAsync(id, cancellationToken);
            if (listing == null)
            {
                throw new KeyNotFoundException("განცხადება ვერ მოიძებნა.");
            }

            if (listing.Status == ListingStatus.Blocked)
            {
                return MapToListing(listing);
            }

            if (listing.Status != ListingStatus.Active)
            {
                throw new InvalidOperationException("მხოლოდ აქტიური განცხადების დაბლოკვაა შესაძლებელი.");
            }

            var now = DateTime.UtcNow;
            listing.Status = ListingStatus.Blocked;
            listing.BlockedAt = now;
            listing.BlockedByUserId = adminUserId;
            listing.BlockReason = request.Reason.Trim();
            listing.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToListing(listing);
        }

        public async Task<MarketplaceListingResponse> UnblockAsync(
            int id,
            int adminUserId,
            CancellationToken cancellationToken = default)
        {
            var listing = await _marketplaceRepository.GetByIdAsync(id, cancellationToken);
            if (listing == null)
            {
                throw new KeyNotFoundException("განცხადება ვერ მოიძებნა.");
            }

            if (listing.Status != ListingStatus.Blocked)
            {
                throw new InvalidOperationException("განცხადება დაბლოკილი არ არის.");
            }

            var now = DateTime.UtcNow;
            if (listing.ExpiresAt <= now)
            {
                listing.Status = ListingStatus.Expired;
                listing.UpdatedAt = now;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                throw new InvalidOperationException("განცხადების ვადა ამოიწურა.");
            }

            listing.Status = ListingStatus.Active;
            listing.BlockedAt = null;
            listing.BlockedByUserId = null;
            listing.BlockReason = null;
            listing.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToListing(listing);
        }

        public async Task<List<MarketplaceListingPhotoResponse>> GetPhotosAsync(
            int listingId,
            CancellationToken cancellationToken = default)
        {
            await EnsureListingExistsAsync(listingId, cancellationToken);
            var photos = await _marketplaceRepository.GetPhotosByListingIdAsync(listingId, cancellationToken);
            return photos.Select(MapPhoto).ToList();
        }

        public async Task<MarketplaceListingPhotoResponse> AddPhotoAsync(
            int listingId,
            Stream stream,
            long length,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            var listing = await GetEditableListingAsync(listingId, cancellationToken);

            if (length <= 0)
            {
                throw new InvalidOperationException("სურათი ცარიელია.");
            }

            if (length > MaxPhotoSizeBytes)
            {
                throw new InvalidOperationException("სურათის ზომა არ უნდა აღემატებოდეს 5 MB-ს.");
            }

            if (!AllowedImageTypes.TryGetValue(contentType, out var extension))
            {
                throw new InvalidOperationException("დასაშვებია მხოლოდ JPEG, PNG ან WebP სურათი.");
            }

            var photoCount = await _marketplaceRepository.CountPhotosByListingIdAsync(
                listingId,
                cancellationToken);

            if (photoCount >= MaxPhotosPerListing)
            {
                throw new InvalidOperationException(
                    $"განცხადებაზე მაქსიმუმ {MaxPhotosPerListing} სურათის დამატებაა შესაძლებელი.");
            }

            var storedFile = await _fileStorage.UploadAsync(
                stream,
                $"marketplace/{listing.Id}/photos",
                extension,
                contentType,
                cancellationToken);

            var photo = new MarketplaceListingPhoto
            {
                ListingId = listingId,
                Key = storedFile.Key,
                ContentType = contentType,
                SizeBytes = length,
                IsPrimary = photoCount == 0,
                CreatedAt = DateTime.UtcNow
            };

            await _marketplaceRepository.AddPhotoAsync(photo, cancellationToken);

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                await _fileStorage.DeleteAsync(storedFile.Key, CancellationToken.None);
                throw;
            }

            return MapPhoto(photo);
        }

        public async Task<MarketplaceListingPhotoResponse> SetPrimaryPhotoAsync(
            int listingId,
            int photoId,
            CancellationToken cancellationToken = default)
        {
            await GetEditableListingAsync(listingId, cancellationToken);

            var photo = await _marketplaceRepository.GetPhotoByIdAsync(photoId, cancellationToken);
            if (photo == null || photo.ListingId != listingId)
            {
                throw new KeyNotFoundException("სურათი ვერ მოიძებნა.");
            }

            var photos = await _marketplaceRepository.GetPhotosByListingIdAsync(listingId, cancellationToken);
            foreach (var item in photos)
            {
                item.IsPrimary = item.Id == photoId;
                item.UpdatedAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapPhoto(photo);
        }

        public async Task DeletePhotoAsync(
            int listingId,
            int photoId,
            CancellationToken cancellationToken = default)
        {
            await GetEditableListingAsync(listingId, cancellationToken);

            var photo = await _marketplaceRepository.GetPhotoByIdAsync(photoId, cancellationToken);
            if (photo == null || photo.ListingId != listingId)
            {
                throw new KeyNotFoundException("სურათი ვერ მოიძებნა.");
            }

            await _fileStorage.DeleteAsync(photo.Key, cancellationToken);

            var wasPrimary = photo.IsPrimary;
            _marketplaceRepository.DeletePhoto(photo);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (wasPrimary)
            {
                var remaining = await _marketplaceRepository.GetPhotosByListingIdAsync(listingId, cancellationToken);
                var nextPrimary = remaining
                    .OrderBy(p => p.CreatedAt)
                    .FirstOrDefault();

                if (nextPrimary != null)
                {
                    nextPrimary.IsPrimary = true;
                    nextPrimary.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
        }

        private async Task<MarketplaceListing> GetEditableListingAsync(
            int id,
            CancellationToken cancellationToken)
        {
            var listing = await _marketplaceRepository.GetByIdAsync(id, cancellationToken);
            if (listing == null)
            {
                throw new KeyNotFoundException("განცხადება ვერ მოიძებნა.");
            }

            if (listing.Status != ListingStatus.Active)
            {
                throw new InvalidOperationException("მხოლოდ აქტიური განცხადების რედაქტირებაა შესაძლებელი.");
            }

            var now = DateTime.UtcNow;
            if (listing.ExpiresAt <= now)
            {
                listing.Status = ListingStatus.Expired;
                listing.UpdatedAt = now;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                throw new InvalidOperationException("განცხადების ვადა ამოიწურა.");
            }

            return listing;
        }

        private async Task EnsureListingExistsAsync(int id, CancellationToken cancellationToken)
        {
            var listing = await _marketplaceRepository.GetByIdAsync(id, cancellationToken);
            if (listing == null)
            {
                throw new KeyNotFoundException("განცხადება ვერ მოიძებნა.");
            }
        }

        private async Task<string?> ValidateLinkedPetAsync(
            int petId,
            int sellerId,
            string? requestedMicrochip,
            int? currentListingId,
            CancellationToken cancellationToken)
        {
            var pet = await _petRepository.GetByIdAsync(petId, cancellationToken);
            if (pet == null)
            {
                throw new KeyNotFoundException("დაკავშირებული ცხოველი ვერ მოიძებნა.");
            }

            if (pet.OwnerId != sellerId)
            {
                throw new UnauthorizedAccessException("მითითებული ცხოველი მომხმარებელს არ ეკუთვნის.");
            }

            if (pet.IsDeceased)
            {
                throw new InvalidOperationException("გარდაცვლილი ცხოველის განცხადებაში გამოყენება შეუძლებელია.");
            }

            if (await _marketplaceRepository.HasActiveListingForPetAsync(
                    petId,
                    currentListingId,
                    cancellationToken))
            {
                throw new InvalidOperationException("ამ ცხოველისთვის უკვე არსებობს აქტიური განცხადება.");
            }

            var petMicrochip = NormalizeMicrochip(pet.MicrochipNumber);
            if (requestedMicrochip != null
                && petMicrochip != null
                && !string.Equals(requestedMicrochip, petMicrochip, StringComparison.Ordinal))
            {
                throw new ArgumentException("მიკროჩიპის ნომერი დაკავშირებული ცხოველის ნომერს არ ემთხვევა.");
            }

            return requestedMicrochip ?? petMicrochip;
        }

        private static void EnsureBusinessRules(ListingType listingType, decimal? price)
        {
            if (listingType == ListingType.Sale)
            {
                if (!price.HasValue || price.Value <= 0)
                {
                    throw new ArgumentException("გასაყიდი ცხოველის ფასი უნდა იყოს დადებითი.");
                }

                if (price.Value > 1_000_000)
                {
                    throw new ArgumentException("ფასი არ უნდა აღემატებოდეს 1 000 000 ლარს.");
                }

                return;
            }

            if (listingType == ListingType.Giveaway && price.HasValue && price.Value != 0)
            {
                throw new ArgumentException("უსასყიდლოდ გადასაცემი ცხოველისთვის ფასი არ უნდა იყოს მითითებული.");
            }
        }

        private static void EnsureMinimumAge(DateTime birthDate)
        {
            if (ToUtc(birthDate).Date > DateTime.UtcNow.Date.AddDays(-MinAgeDays))
            {
                throw new ArgumentException(
                    "განცხადების გამოქვეყნებისთვის ცხოველის ასაკი უნდა იყოს მინიმუმ 8 კვირა.");
            }
        }

        private static DateTime NormalizeDate(DateTime value)
        {
            return DateTime.SpecifyKind(ToUtc(value).Date, DateTimeKind.Utc);
        }

        private static DateTime ToUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();
        }

        private static decimal? NormalizePrice(ListingType listingType, decimal? price)
        {
            if (listingType == ListingType.Giveaway)
            {
                return null;
            }

            return price;
        }

        private static string? NormalizeMicrochip(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }

        private static string? TrimOrNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static (int Page, int PageSize) NormalizePagination(int page, int pageSize)
        {
            var validPage = page < 1 ? 1 : page;
            var validPageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);
            return (validPage, validPageSize);
        }

        private PagedResult<MarketplaceListingResponse> ToPagedResult(
            List<MarketplaceListing> listings,
            int page,
            int pageSize,
            int totalCount)
        {
            return new PagedResult<MarketplaceListingResponse>
            {
                Items = listings.Select(l => MapToListing(l)).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        private MarketplaceListingResponse MapToListing(
            MarketplaceListing listing,
            List<MarketplaceListingPhoto>? photos = null)
        {
            var listingPhotos = photos ?? listing.Photos.ToList();
            var ageDays = Math.Max(0, (DateTime.UtcNow.Date - listing.BirthDate.Date).Days);

            return new MarketplaceListingResponse
            {
                Id = listing.Id,
                SellerId = listing.SellerId,
                PetId = listing.PetId,
                PetName = listing.PetName,
                Species = listing.Species,
                Breed = listing.Breed,
                Gender = listing.Gender,
                BirthDate = listing.BirthDate,
                AgeWeeks = ageDays / 7,
                Color = listing.Color,
                Size = listing.Size,
                IsSterilized = listing.IsSterilized,
                IsVaccinated = listing.IsVaccinated,
                MicrochipNumber = listing.MicrochipNumber,
                ListingType = listing.ListingType,
                Price = listing.Price,
                City = listing.City,
                ContactPhone = listing.ContactPhone,
                Description = listing.Description,
                Status = listing.Status,
                ExpiresAt = listing.ExpiresAt,
                CompletedAt = listing.CompletedAt,
                BlockedAt = listing.BlockedAt,
                BlockReason = listing.BlockReason,
                WithdrawnAt = listing.WithdrawnAt,
                CreatedAt = listing.CreatedAt,
                Photos = listingPhotos
                    .OrderByDescending(p => p.IsPrimary)
                    .ThenBy(p => p.CreatedAt)
                    .Select(MapPhoto)
                    .ToList()
            };
        }

        private MarketplaceListingPhotoResponse MapPhoto(MarketplaceListingPhoto photo)
        {
            return new MarketplaceListingPhotoResponse
            {
                Id = photo.Id,
                ListingId = photo.ListingId,
                Url = _fileStorage.GetUrl(photo.Key),
                IsPrimary = photo.IsPrimary,
                CreatedAt = photo.CreatedAt
            };
        }

        private static OwnershipTransferResponse MapTransfer(OwnershipTransfer transfer)
        {
            return new OwnershipTransferResponse
            {
                Id = transfer.Id,
                ListingId = transfer.ListingId,
                PetId = transfer.PetId,
                MicrochipNumber = transfer.MicrochipNumber,
                SellerId = transfer.SellerId,
                BuyerUserId = transfer.BuyerUserId,
                BuyerFullName = transfer.BuyerFullName,
                BuyerPersonalId = transfer.BuyerPersonalId,
                BuyerPhone = transfer.BuyerPhone,
                TransferType = transfer.TransferType,
                Price = transfer.Price,
                Status = transfer.Status,
                TransferredAt = transfer.TransferredAt,
                AirsSyncedAt = transfer.AirsSyncedAt,
                AirsReference = transfer.AirsReference,
                Notes = transfer.Notes,
                CreatedAt = transfer.CreatedAt
            };
        }
    }
}
