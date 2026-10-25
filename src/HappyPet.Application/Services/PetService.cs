using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Services;
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
        private readonly IPetRepository _petRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PetService(IPetRepository petRepository, IUnitOfWork unitOfWork)
        {
            _petRepository = petRepository;
            _unitOfWork = unitOfWork;
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

            _petRepository.Delete(pet);
            await _unitOfWork.SaveChangesAsync();
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

        private static PetResponse MapToResponse(Pet pet)
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
                CreatedAt = pet.CreatedAt
            };
        }
    }
}
