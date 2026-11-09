using System.Globalization;
using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Dtos.Responses;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Domain.Entities;

namespace HappyPet.Application.Services
{
    public class VaccinationService : IVaccinationService
    {
        private const int DefaultDueDays = 30;
        private const int MaxDueDays = 365;

        private readonly IVaccinationRepository _vaccinationRepository;
        private readonly IPetRepository _petRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAirsService _airsService;

        public VaccinationService(
            IVaccinationRepository vaccinationRepository,
            IPetRepository petRepository,
            IUnitOfWork unitOfWork,
            IAirsService airsService)
        {
            _vaccinationRepository = vaccinationRepository;
            _petRepository = petRepository;
            _unitOfWork = unitOfWork;
            _airsService = airsService;
        }

        public async Task<List<VaccinationResponse>> GetByPetIdAsync(
            int petId,
            CancellationToken cancellationToken = default)
        {
            var pet = await GetPetAsync(petId, cancellationToken);

            var vaccinations = await _vaccinationRepository.GetByPetIdAsync(petId, cancellationToken);
            return vaccinations.Select(v => MapToResponse(v, pet.Name)).ToList();
        }

        public async Task<List<VaccinationResponse>> GetDueAsync(
            int ownerId,
            int daysAhead = 30,
            CancellationToken cancellationToken = default)
        {
            var validDays = daysAhead switch
            {
                < 1 => DefaultDueDays,
                > MaxDueDays => MaxDueDays,
                _ => daysAhead
            };

            var toUtc = DateTime.UtcNow.Date.AddDays(validDays);
            var vaccinations = await _vaccinationRepository.GetDueAsync(ownerId, toUtc, cancellationToken);

            return vaccinations.Select(v => MapToResponse(v, v.Pet?.Name ?? string.Empty)).ToList();
        }

        public async Task<VaccinationResponse> CreateAsync(
            int petId,
            CreateVaccinationRequest request,
            CancellationToken cancellationToken = default)
        {
            var pet = await GetPetAsync(petId, cancellationToken);
            var vaccinatedAt = ToUtc(request.VaccinatedAt);
            var nextDueAt = ToUtcOrNull(request.NextDueAt);
            EnsureValidDueDate(vaccinatedAt, nextDueAt);

            var vaccination = new Vaccination
            {
                PetId = petId,
                VaccineName = request.VaccineName.Trim(),
                Disease = TrimOrNull(request.Disease),
                Manufacturer = TrimOrNull(request.Manufacturer),
                BatchNumber = TrimOrNull(request.BatchNumber),
                CertificateNumber = TrimOrNull(request.CertificateNumber),
                VaccinatedAt = vaccinatedAt,
                NextDueAt = nextDueAt,
                VeterinarianName = TrimOrNull(request.VeterinarianName),
                ClinicName = TrimOrNull(request.ClinicName),
                Notes = TrimOrNull(request.Notes),
                CreatedAt = DateTime.UtcNow
            };

            await _vaccinationRepository.AddAsync(vaccination, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(vaccination, pet.Name);
        }

        public async Task<VaccinationResponse> UpdateAsync(
            int petId,
            int vaccinationId,
            UpdateVaccinationRequest request,
            CancellationToken cancellationToken = default)
        {
            var pet = await GetPetAsync(petId, cancellationToken);
            var vaccination = await _vaccinationRepository.GetByIdAsync(vaccinationId, cancellationToken);

            if (vaccination == null || vaccination.PetId != petId)
            {
                throw new KeyNotFoundException("ვაქცინაციის ჩანაწერი ვერ მოიძებნა.");
            }

            var vaccinatedAt = ToUtc(request.VaccinatedAt);
            var nextDueAt = ToUtcOrNull(request.NextDueAt);
            EnsureValidDueDate(vaccinatedAt, nextDueAt);

            vaccination.VaccineName = request.VaccineName.Trim();
            vaccination.Disease = TrimOrNull(request.Disease);
            vaccination.Manufacturer = TrimOrNull(request.Manufacturer);
            vaccination.BatchNumber = TrimOrNull(request.BatchNumber);
            vaccination.CertificateNumber = TrimOrNull(request.CertificateNumber);
            vaccination.VaccinatedAt = vaccinatedAt;
            vaccination.NextDueAt = nextDueAt;
            vaccination.VeterinarianName = TrimOrNull(request.VeterinarianName);
            vaccination.ClinicName = TrimOrNull(request.ClinicName);
            vaccination.Notes = TrimOrNull(request.Notes);
            vaccination.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(vaccination, pet.Name);
        }

        public async Task DeleteAsync(int petId, int vaccinationId, CancellationToken cancellationToken = default)
        {
            await GetPetAsync(petId, cancellationToken);
            var vaccination = await _vaccinationRepository.GetByIdAsync(vaccinationId, cancellationToken);

            if (vaccination == null || vaccination.PetId != petId)
            {
                throw new KeyNotFoundException("ვაქცინაციის ჩანაწერი ვერ მოიძებნა.");
            }

            _vaccinationRepository.Delete(vaccination);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<VaccinationImportResult> ImportFromAirsAsync(
            int petId,
            CancellationToken cancellationToken = default)
        {
            var pet = await GetPetAsync(petId, cancellationToken);

            if (string.IsNullOrWhiteSpace(pet.MicrochipNumber))
            {
                throw new InvalidOperationException(
                    "AIRS-იდან ვაქცინაციების იმპორტისთვის ცხოველს უნდა ჰქონდეს მიკროჩიპის ნომერი.");
            }

            var details = await _airsService.GetAnimalByMicrochipAsync(pet.MicrochipNumber, cancellationToken);
            var existing = await _vaccinationRepository.GetByPetIdAsync(petId, cancellationToken);
            var existingAirsIds = existing
                .Where(v => !string.IsNullOrWhiteSpace(v.AirsVaccinationId))
                .Select(v => v.AirsVaccinationId!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var imported = new List<Vaccination>();
            var skipped = 0;

            foreach (var source in details.Vaccinations)
            {
                var airsId = source.Id?.Trim();
                if (string.IsNullOrWhiteSpace(airsId)
                    || !existingAirsIds.Add(airsId)
                    || !TryParseAirsDate(source.AdministeredAt, out var administeredAt)
                    || string.IsNullOrWhiteSpace(source.VaccineName))
                {
                    skipped++;
                    continue;
                }

                var nextDueAt = TryParseAirsDate(source.NextDueAt, out var dueDate) ? dueDate : (DateTime?)null;

                imported.Add(new Vaccination
                {
                    PetId = petId,
                    VaccineName = source.VaccineName.Trim(),
                    Disease = TrimOrNull(source.Disease),
                    Manufacturer = TrimOrNull(source.Manufacturer),
                    BatchNumber = TrimOrNull(source.BatchNumber),
                    CertificateNumber = TrimOrNull(source.CertificateNumber),
                    VaccinatedAt = administeredAt,
                    NextDueAt = nextDueAt,
                    VeterinarianName = TrimOrNull(source.VeterinarianName),
                    ClinicName = TrimOrNull(source.ClinicName),
                    AirsVaccinationId = airsId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (imported.Count > 0)
            {
                await _vaccinationRepository.AddRangeAsync(imported, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return new VaccinationImportResult
            {
                Imported = imported.Count,
                Skipped = skipped,
                Items = imported
                    .OrderByDescending(v => v.VaccinatedAt)
                    .Select(v => MapToResponse(v, pet.Name))
                    .ToList()
            };
        }

        private async Task<Pet> GetPetAsync(int petId, CancellationToken cancellationToken)
        {
            var pet = await _petRepository.GetByIdAsync(petId, cancellationToken);
            if (pet == null)
            {
                throw new KeyNotFoundException("ცხოველი ვერ მოიძებნა.");
            }

            return pet;
        }

        private static void EnsureValidDueDate(DateTime vaccinatedAt, DateTime? nextDueAt)
        {
            if (nextDueAt.HasValue && nextDueAt.Value <= vaccinatedAt)
            {
                throw new ArgumentException("შემდეგი ვაქცინაციის თარიღი უნდა იყოს ვაქცინაციის თარიღზე გვიან.");
            }
        }

        private static VaccinationResponse MapToResponse(Vaccination vaccination, string petName)
        {
            var daysUntilDue = vaccination.NextDueAt.HasValue
                ? (vaccination.NextDueAt.Value.Date - DateTime.UtcNow.Date).Days
                : (int?)null;

            return new VaccinationResponse
            {
                Id = vaccination.Id,
                PetId = vaccination.PetId,
                PetName = petName,
                VaccineName = vaccination.VaccineName,
                Disease = vaccination.Disease,
                Manufacturer = vaccination.Manufacturer,
                BatchNumber = vaccination.BatchNumber,
                CertificateNumber = vaccination.CertificateNumber,
                VaccinatedAt = vaccination.VaccinatedAt,
                NextDueAt = vaccination.NextDueAt,
                DaysUntilDue = daysUntilDue,
                IsOverdue = vaccination.NextDueAt.HasValue
                            && vaccination.NextDueAt.Value.Date < DateTime.UtcNow.Date,
                VeterinarianName = vaccination.VeterinarianName,
                ClinicName = vaccination.ClinicName,
                Notes = vaccination.Notes,
                AirsVaccinationId = vaccination.AirsVaccinationId,
                CreatedAt = vaccination.CreatedAt
            };
        }

        private static bool TryParseAirsDate(string? value, out DateTime result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out result);
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

        private static DateTime? ToUtcOrNull(DateTime? value)
        {
            return value.HasValue ? ToUtc(value.Value) : null;
        }
    }
}
