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

        private static readonly Dictionary<string, string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp"
        };

        private readonly IPetRepository _petRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorage;

        public PetService(
            IPetRepository petRepository,
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorage)
        {
            _petRepository = petRepository;
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
        }

        public async Task<PetResponse> CreateAsync(Guid ownerId, CreatePetRequest request, CancellationToken cancellationToken = default)
        {
            var microchipNumber = request.MicrochipNumber.Trim();

            if (await _petRepository.ExistsByMicrochipNumberAsync(microchipNumber, cancellationToken))
            {
                throw new InvalidOperationException("მითითებული მიკროჩიპის ნომრით ცხოველი უკვე რეგისტრირებულია.");
            }

            var newPet = new Pet
            {
                Id = Guid.NewGuid(),
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
                CreatedAt = DateTime.UtcNow
            };

            await _petRepository.AddAsync(newPet, cancellationToken);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(newPet);
        }

        public async Task<PetResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var pet = await _petRepository.GetByIdAsync(id, cancellationToken);
            return pet == null ? null : MapToResponse(pet);
        }

        public async Task<List<PetResponse>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
        {
            var pets = await _petRepository.GetByOwnerIdAsync(ownerId, cancellationToken);
            return pets.Select(MapToResponse).ToList();
        }

        public async Task<List<PetResponse>> GetLostAsync(CancellationToken cancellationToken = default)
        {
            var pets = await _petRepository.GetLostAsync(cancellationToken);
            return pets.Select(MapToResponse).ToList();
        }

        public async Task<PetResponse> UpdateAsync(Guid id, UpdatePetRequest request, CancellationToken cancellationToken = default)
        {
            var pet = await _petRepository.GetByIdAsync(id, cancellationToken);
            if (pet == null)
            {
                throw new KeyNotFoundException("ცხოველი ვერ მოიძებნა.");
            }

            var microchipNumber = request.MicrochipNumber.Trim();

            if (pet.MicrochipNumber != microchipNumber &&
                await _petRepository.ExistsByMicrochipNumberAsync(microchipNumber, cancellationToken))
            {
                throw new InvalidOperationException("მითითებული მიკროჩიპის ნომრით ცხოველი უკვე რეგისტრირებულია.");
            }

            pet.Name = request.Name.Trim();
            pet.Species = request.Species;
            pet.Breed = request.Breed?.Trim();
            pet.MicrochipNumber = microchipNumber;
            pet.BirthDate = ToUtcOrNull(request.BirthDate);
            pet.Gender = request.Gender;
            pet.Color = request.Color?.Trim();
            pet.DistinguishingMarks = request.DistinguishingMarks?.Trim();
            pet.IsLost = request.IsLost;
            pet.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(pet);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
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

        public async Task<List<PetPhotoResponse>> GetPhotosAsync(Guid petId, CancellationToken cancellationToken = default)
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
            Guid petId,
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
                Id = Guid.NewGuid(),
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

        public async Task<PetPhotoResponse> SetPrimaryPhotoAsync(Guid petId, Guid photoId, CancellationToken cancellationToken = default)
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

        public async Task DeletePhotoAsync(Guid petId, Guid photoId, CancellationToken cancellationToken = default)
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
    }
}
