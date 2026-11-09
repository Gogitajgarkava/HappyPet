using HappyPet.Application.Common;
using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Application.Interfaces.Storage;
using HappyPet.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HappyPet.Application.Services
{
    public class PetService : IPetService
    {
        private const long MaxPhotoSizeBytes = 5 * 1024 * 1024;
        private const int MaxPhotosPerPet = 10;
        private const int DefaultPageSize = 20;
        private const int MaxPageSize = 100;
        private const double DefaultRadiusKm = 25;
        private const double MaxRadiusKm = 100;

        private static readonly Dictionary<string, string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp"
        };

        private readonly IPetRepository _petRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorage;
        private readonly IUserRepository _userRepository;
        private readonly IAirsService _airsService;

        public PetService(
            IPetRepository petRepository,
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorage,
            IUserRepository userRepository,
            IAirsService airsService)
        {
            _petRepository = petRepository;
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
            _userRepository = userRepository;
            _airsService = airsService;
        }

        public async Task<PetResponse> CreateAsync(int ownerId, CreatePetRequest request, CancellationToken cancellationToken = default)
        {
            var microchipNumber = request.MicrochipNumber.Trim();

            await EnsureMicrochipCanBeRegisteredAsync(ownerId, microchipNumber, cancellationToken);

            var newPet = new Pet
            {
                OwnerId = ownerId,
                Name = request.Name.Trim(),
                Species = request.Species,
                Breed = request.Breed?.Trim(),
                MicrochipNumber = microchipNumber,
                BirthDate = ToUtcOrNull(request.BirthDate),
                Gender = request.Gender,
                Color = request.Color?.Trim(),
                DistinguishingMarks = request.DistinguishingMarks?.Trim(),
                IsLost = request.IsLost,
                LostAt = request.IsLost ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow
            };

            await _petRepository.AddAsync(newPet, cancellationToken);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(newPet);
        }

        public async Task<PetResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var pet = await _petRepository.GetByIdAsync(id, cancellationToken);
            return pet == null ? null : MapToResponse(pet);
        }

        public async Task<PagedResult<PetResponse>> GetByOwnerIdAsync(
            int ownerId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var (validPage, validPageSize) = NormalizePagination(page, pageSize);
            var (pets, totalCount) = await _petRepository.GetByOwnerIdPagedAsync(
                ownerId,
                (validPage - 1) * validPageSize,
                validPageSize,
                cancellationToken);

            return ToPagedResult(pets, validPage, validPageSize, totalCount);
        }

        public async Task<PagedResult<LostPetResponse>> GetLostAsync(
            int page = 1,
            int pageSize = 20,
            double? latitude = null,
            double? longitude = null,
            double? radiusKm = null,
            CancellationToken cancellationToken = default)
        {
            var (validPage, validPageSize) = NormalizePagination(page, pageSize);
            var skip = (validPage - 1) * validPageSize;

            if (latitude.HasValue || longitude.HasValue)
            {
                if (!latitude.HasValue || !longitude.HasValue)
                {
                    throw new ArgumentException("განედი და გრძედი ერთად უნდა იყოს მითითებული.");
                }

                if (latitude is < -90 or > 90)
                {
                    throw new ArgumentException("განედი უნდა იყოს -90-დან 90-მდე.");
                }

                if (longitude is < -180 or > 180)
                {
                    throw new ArgumentException("გრძედი უნდა იყოს -180-დან 180-მდე.");
                }

                var validRadius = NormalizeRadius(radiusKm);
                var box = GeoDistance.GetBoundingBox(latitude.Value, longitude.Value, validRadius);

                var candidates = await _petRepository.GetLostWithinBoundingBoxAsync(
                    box.MinLatitude,
                    box.MaxLatitude,
                    box.MinLongitude,
                    box.MaxLongitude,
                    box.CrossesAntimeridian,
                    cancellationToken);

                var matched = candidates
                    .Where(p => p.LastSeenLatitude.HasValue && p.LastSeenLongitude.HasValue)
                    .Select(p => new
                    {
                        Pet = p,
                        DistanceKm = GeoDistance.CalculateKm(
                            latitude.Value,
                            longitude.Value,
                            p.LastSeenLatitude!.Value,
                            p.LastSeenLongitude!.Value)
                    })
                    .Where(x => x.DistanceKm <= validRadius)
                    .OrderBy(x => x.DistanceKm)
                    .ThenByDescending(x => x.Pet.LostAt ?? x.Pet.CreatedAt)
                    .ToList();

                var items = matched
                    .Skip(skip)
                    .Take(validPageSize)
                    .Select(x => MapToLostResponse(x.Pet, Math.Round(x.DistanceKm, 2)))
                    .ToList();

                return new PagedResult<LostPetResponse>
                {
                    Items = items,
                    Page = validPage,
                    PageSize = validPageSize,
                    TotalCount = matched.Count
                };
            }

            var (pets, totalCount) = await _petRepository.GetLostPagedAsync(
                skip,
                validPageSize,
                cancellationToken);

            return new PagedResult<LostPetResponse>
            {
                Items = pets.Select(pet => MapToLostResponse(pet, null)).ToList(),
                Page = validPage,
                PageSize = validPageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PetResponse> ReportLostAsync(
            int id,
            ReportLostPetRequest request,
            CancellationToken cancellationToken = default)
        {
            var pet = await _petRepository.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                throw new KeyNotFoundException("ცხოველი ვერ მოიძებნა.");
            }

            var now = DateTime.UtcNow;

            if (!pet.IsLost)
            {
                pet.LostAt = now;
            }

            pet.IsLost = true;
            pet.FoundAt = null;
            pet.LastSeenAt = ToUtc(request.LastSeenAt);
            pet.LastSeenLatitude = request.Latitude;
            pet.LastSeenLongitude = request.Longitude;
            pet.LastSeenAddress = TrimOrNull(request.LastSeenAddress);
            pet.ContactPhone = TrimOrNull(request.ContactPhone);
            pet.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(pet);
        }

        public async Task<PetResponse> ReportFoundAsync(int id, CancellationToken cancellationToken = default)
        {
            var pet = await _petRepository.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                throw new KeyNotFoundException("ცხოველი ვერ მოიძებნა.");
            }

            if (pet.IsLost)
            {
                pet.IsLost = false;
                pet.FoundAt = DateTime.UtcNow;
                pet.UpdatedAt = pet.FoundAt;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return MapToResponse(pet);
        }

        public async Task<PetResponse> UpdateAsync(int id, UpdatePetRequest request, CancellationToken cancellationToken = default)
        {
            var pet = await _petRepository.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                throw new KeyNotFoundException("ცხოველი ვერ მოიძებნა.");
            }

            var microchipNumber = request.MicrochipNumber.Trim();

            if (!string.Equals(pet.MicrochipNumber, microchipNumber, StringComparison.Ordinal))
            {
                await EnsureMicrochipCanBeRegisteredAsync(pet.OwnerId, microchipNumber, cancellationToken);
            }

            pet.Name = request.Name.Trim();
            pet.Species = request.Species;
            pet.Breed = request.Breed?.Trim();
            pet.MicrochipNumber = microchipNumber;
            pet.BirthDate = ToUtcOrNull(request.BirthDate);
            pet.Gender = request.Gender;
            pet.Color = request.Color?.Trim();
            pet.DistinguishingMarks = request.DistinguishingMarks?.Trim();

            var now = DateTime.UtcNow;
            if (request.IsLost && !pet.IsLost)
            {
                pet.LostAt = now;
                pet.FoundAt = null;
            }
            else if (!request.IsLost && pet.IsLost)
            {
                pet.FoundAt = now;
            }

            pet.IsLost = request.IsLost;
            pet.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(pet);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var pet = await _petRepository.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                throw new KeyNotFoundException("ცხოველი ვერ მოიძებნა.");
            }

            var photos = await _petRepository.GetPhotosByPetIdAsync(id, cancellationToken);
            foreach (var photo in photos)
            {
                await _fileStorage.DeleteAsync(photo.Key, cancellationToken);
            }

            _petRepository.Delete(pet);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<PetPhotoResponse>> GetPhotosAsync(int petId, CancellationToken cancellationToken = default)
        {
            var pet = await _petRepository.GetByIdAsync(petId, cancellationToken);
            if (pet == null)
            {
                throw new KeyNotFoundException("ცხოველი ვერ მოიძებნა.");
            }

            var photos = await _petRepository.GetPhotosByPetIdAsync(petId, cancellationToken);
            return photos.Select(MapPhoto).ToList();
        }

        public async Task<PetPhotoResponse> AddPhotoAsync(
            int petId,
            Stream stream,
            long length,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            var pet = await _petRepository.GetByIdAsync(petId, cancellationToken);
            if (pet == null)
            {
                throw new KeyNotFoundException("ცხოველი ვერ მოიძებნა.");
            }

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

            var photoCount = await _petRepository.CountPhotosByPetIdAsync(petId, cancellationToken);
            if (photoCount >= MaxPhotosPerPet)
            {
                throw new InvalidOperationException($"ცხოველზე მაქსიმუმ {MaxPhotosPerPet} სურათის დამატებაა შესაძლებელი.");
            }

            var storedFile = await _fileStorage.UploadAsync(
                stream,
                $"pets/{petId}/photos",
                extension,
                contentType,
                cancellationToken);

            var photo = new PetPhoto
            {
                PetId = petId,
                Key = storedFile.Key,
                ContentType = contentType,
                SizeBytes = length,
                IsPrimary = photoCount == 0,
                CreatedAt = DateTime.UtcNow
            };

            await _petRepository.AddPhotoAsync(photo, cancellationToken);

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

        public async Task<PetPhotoResponse> SetPrimaryPhotoAsync(int petId, int photoId, CancellationToken cancellationToken = default)
        {
            var pet = await _petRepository.GetByIdAsync(petId, cancellationToken);
            if (pet == null)
            {
                throw new KeyNotFoundException("ცხოველი ვერ მოიძებნა.");
            }

            var photo = await _petRepository.GetPhotoByIdAsync(photoId, cancellationToken);
            if (photo == null || photo.PetId != petId)
            {
                throw new KeyNotFoundException("სურათი ვერ მოიძებნა.");
            }

            var photos = await _petRepository.GetPhotosByPetIdAsync(petId, cancellationToken);
            foreach (var item in photos)
            {
                item.IsPrimary = item.Id == photoId;
                item.UpdatedAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapPhoto(photo);
        }

        public async Task DeletePhotoAsync(int petId, int photoId, CancellationToken cancellationToken = default)
        {
            var pet = await _petRepository.GetByIdAsync(petId, cancellationToken);
            if (pet == null)
            {
                throw new KeyNotFoundException("ცხოველი ვერ მოიძებნა.");
            }

            var photo = await _petRepository.GetPhotoByIdAsync(photoId, cancellationToken);
            if (photo == null || photo.PetId != petId)
            {
                throw new KeyNotFoundException("სურათი ვერ მოიძებნა.");
            }

            await _fileStorage.DeleteAsync(photo.Key, cancellationToken);

            var wasPrimary = photo.IsPrimary;
            _petRepository.DeletePhoto(photo);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (wasPrimary)
            {
                var remaining = await _petRepository.GetPhotosByPetIdAsync(petId, cancellationToken);
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

        private async Task EnsureMicrochipCanBeRegisteredAsync(
            int ownerId,
            string microchipNumber,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(microchipNumber))
            {
                return;
            }

            if (await _petRepository.ExistsByMicrochipNumberAsync(microchipNumber, cancellationToken))
            {
                throw new InvalidOperationException("მითითებული მიკროჩიპის ნომრით ცხოველი უკვე რეგისტრირებულია.");
            }

            var owner = await _userRepository.GetByIdAsync(ownerId, cancellationToken);
            if (owner == null)
            {
                throw new InvalidOperationException(
                    "მომხმარებელი ვერ მოიძებნა. მიკროჩიპის მფლობელის შემოწმება შეუძლებელია.");
            }

            await _airsService.EnsureMicrochipBelongsToOwnerAsync(
                microchipNumber,
                owner.PersonalId,
                cancellationToken);
        }

        private static (int Page, int PageSize) NormalizePagination(int page, int pageSize)
        {
            var validPage = page < 1 ? 1 : page;
            var validPageSize = pageSize switch
            {
                < 1 => DefaultPageSize,
                > MaxPageSize => MaxPageSize,
                _ => pageSize
            };

            return (validPage, validPageSize);
        }

        private static double NormalizeRadius(double? radiusKm)
        {
            return radiusKm switch
            {
                null or <= 0 => DefaultRadiusKm,
                > MaxRadiusKm => MaxRadiusKm,
                _ => radiusKm.Value
            };
        }

        private static string? TrimOrNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static DateTime ToUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();
        }

        private PagedResult<PetResponse> ToPagedResult(
            List<Pet> pets,
            int page,
            int pageSize,
            int totalCount)
        {
            return new PagedResult<PetResponse>
            {
                Items = pets.Select(MapToResponse).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        private static DateTime? ToUtcOrNull(DateTime? value)
        {
            if (value == null)
            {
                return null;
            }

            return value.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                : value.Value.ToUniversalTime();
        }

        private PetResponse MapToResponse(Pet pet)
        {
            return new PetResponse
            {
                Id = pet.Id,
                OwnerId = pet.OwnerId,
                Name = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                MicrochipNumber = pet.MicrochipNumber,
                BirthDate = pet.BirthDate,
                Gender = pet.Gender,
                Color = pet.Color,
                DistinguishingMarks = pet.DistinguishingMarks,
                IsLost = pet.IsLost,
                LostAt = pet.LostAt,
                FoundAt = pet.FoundAt,
                LastSeenAt = pet.LastSeenAt,
                LastSeenLatitude = pet.LastSeenLatitude,
                LastSeenLongitude = pet.LastSeenLongitude,
                LastSeenAddress = pet.LastSeenAddress,
                ContactPhone = pet.ContactPhone,
                CreatedAt = pet.CreatedAt,
                Photos = pet.Photos
                    .OrderByDescending(p => p.IsPrimary)
                    .ThenBy(p => p.CreatedAt)
                    .Select(MapPhoto)
                    .ToList()
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

        private LostPetResponse MapToLostResponse(Pet pet, double? distanceKm)
        {
            return new LostPetResponse
            {
                Id = pet.Id,
                Name = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                Gender = pet.Gender,
                Color = pet.Color,
                DistinguishingMarks = pet.DistinguishingMarks,
                LostAt = pet.LostAt,
                LastSeenAt = pet.LastSeenAt,
                LastSeenLatitude = pet.LastSeenLatitude,
                LastSeenLongitude = pet.LastSeenLongitude,
                LastSeenAddress = pet.LastSeenAddress,
                ContactPhone = pet.ContactPhone,
                DistanceKm = distanceKm,
                CreatedAt = pet.CreatedAt,
                Photos = pet.Photos
                    .OrderByDescending(p => p.IsPrimary)
                    .ThenBy(p => p.CreatedAt)
                    .Select(MapPhoto)
                    .ToList()
            };
        }
    }
}
