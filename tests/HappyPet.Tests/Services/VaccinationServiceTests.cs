using HappyPet.Application.Dtos.Airs;
using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Application.Services;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;
using Moq;

namespace HappyPet.Tests.Services;

public class VaccinationServiceTests
{
    private readonly Mock<IVaccinationRepository> _vaccinationRepository = new();
    private readonly Mock<IPetRepository> _petRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAirsService> _airsService = new();

    public VaccinationServiceTests()
    {
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _vaccinationRepository
            .Setup(r => r.GetByPetIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vaccination>());
    }

    private VaccinationService CreateService() => new(
        _vaccinationRepository.Object,
        _petRepository.Object,
        _unitOfWork.Object,
        _airsService.Object);

    [Fact]
    public async Task CreateAsync_MapsRequestAndSaves()
    {
        _petRepository
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildPet());

        var vaccinatedAt = DateTime.UtcNow.AddDays(-10);
        var request = new CreateVaccinationRequest
        {
            VaccineName = "  Rabies  ",
            Disease = "  რაბიესი  ",
            Manufacturer = "MSD",
            BatchNumber = "B-1",
            VaccinatedAt = vaccinatedAt,
            NextDueAt = vaccinatedAt.AddYears(1),
            VeterinarianName = "ექიმი",
            ClinicName = "კლინიკა",
            Notes = "  კარგად  "
        };

        var result = await CreateService().CreateAsync(7, request);

        Assert.Equal("Rabies", result.VaccineName);
        Assert.Equal("რაბიესი", result.Disease);
        Assert.Equal("კარგად", result.Notes);
        Assert.Equal(7, result.PetId);
        Assert.Equal("შერი", result.PetName);
        Assert.NotNull(result.DaysUntilDue);
        _vaccinationRepository.Verify(
            r => r.AddAsync(It.IsAny<Vaccination>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenNextDueIsNotAfterVaccination_ThrowsArgumentException()
    {
        _petRepository
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildPet());

        var vaccinatedAt = DateTime.UtcNow.AddDays(-10);
        var request = new CreateVaccinationRequest
        {
            VaccineName = "Rabies",
            VaccinatedAt = vaccinatedAt,
            NextDueAt = vaccinatedAt.AddDays(-1)
        };

        await Assert.ThrowsAsync<ArgumentException>(() => CreateService().CreateAsync(7, request));
    }

    [Fact]
    public async Task CreateAsync_WhenPetDoesNotExist_ThrowsKeyNotFound()
    {
        _petRepository
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pet?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CreateService().CreateAsync(7, BuildCreateRequest()));
    }

    [Fact]
    public async Task UpdateAsync_WhenVaccinationBelongsToAnotherPet_ThrowsKeyNotFound()
    {
        _petRepository
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildPet());
        _vaccinationRepository
            .Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Vaccination { Id = 3, PetId = 99, VaccineName = "Rabies" });

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CreateService().UpdateAsync(7, 3, BuildUpdateRequest()));
    }

    [Fact]
    public async Task DeleteAsync_RemovesVaccination()
    {
        var vaccination = new Vaccination { Id = 3, PetId = 7, VaccineName = "Rabies" };
        _petRepository
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildPet());
        _vaccinationRepository
            .Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaccination);

        await CreateService().DeleteAsync(7, 3);

        _vaccinationRepository.Verify(r => r.Delete(vaccination), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDueAsync_ClampsDaysAndMapsOverdueState()
    {
        var overdue = new Vaccination
        {
            Id = 1,
            PetId = 7,
            VaccineName = "Rabies",
            VaccinatedAt = DateTime.UtcNow.AddYears(-2),
            NextDueAt = DateTime.UtcNow.AddDays(-5),
            Pet = BuildPet()
        };

        DateTime? requestedTo = null;
        _vaccinationRepository
            .Setup(r => r.GetDueAsync(1, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Callback<int, DateTime, CancellationToken>((_, to, _) => requestedTo = to)
            .ReturnsAsync(new List<Vaccination> { overdue });

        var result = await CreateService().GetDueAsync(1, daysAhead: 5000);

        Assert.Single(result);
        Assert.True(result[0].IsOverdue);
        Assert.Equal("შერი", result[0].PetName);
        Assert.NotNull(requestedTo);
        Assert.True(requestedTo > DateTime.UtcNow.AddDays(300));
    }

    [Fact]
    public async Task ImportFromAirsAsync_ImportsNewAndSkipsExistingAndInvalid()
    {
        var pet = BuildPet();
        _petRepository
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pet);
        _vaccinationRepository
            .Setup(r => r.GetByPetIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vaccination>
            {
                new() { Id = 1, PetId = 7, VaccineName = "Rabies", AirsVaccinationId = "vac-1" }
            });
        _airsService
            .Setup(a => a.GetAnimalByMicrochipAsync("982000000000001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AirsAnimalDetails
            {
                Animal = new AirsAnimal { MicrochipNumber = pet.MicrochipNumber },
                Vaccinations = new List<AirsVaccination>
                {
                    new()
                    {
                        Id = "vac-1",
                        VaccineName = "Rabies",
                        AdministeredAt = "2025-01-10",
                        NextDueAt = "2026-01-10"
                    },
                    new()
                    {
                        Id = "vac-2",
                        VaccineName = "Bordetella",
                        AdministeredAt = "2025-02-15",
                        NextDueAt = "2026-02-15",
                        ClinicName = "კლინიკა"
                    },
                    new()
                    {
                        Id = "vac-3",
                        VaccineName = "Invalid date",
                        AdministeredAt = "not-a-date"
                    }
                }
            });

        var result = await CreateService().ImportFromAirsAsync(7);

        Assert.Equal(1, result.Imported);
        Assert.Equal(2, result.Skipped);
        Assert.Single(result.Items);
        Assert.Equal("Bordetella", result.Items[0].VaccineName);
        Assert.Equal("vac-2", result.Items[0].AirsVaccinationId);
        _vaccinationRepository.Verify(
            r => r.AddRangeAsync(
                It.Is<IEnumerable<Vaccination>>(items =>
                    items.Count() == 1 && items.Single().AirsVaccinationId == "vac-2"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ImportFromAirsAsync_WhenMicrochipIsMissing_ThrowsInvalidOperation()
    {
        var pet = BuildPet();
        pet.MicrochipNumber = string.Empty;
        _petRepository
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pet);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().ImportFromAirsAsync(7));
    }

    [Fact]
    public async Task CreateAsync_WithImmunityYears_ComputesNextDueAt()
    {
        _petRepository
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildPet());
        var vaccinatedAt = DateTime.UtcNow.AddDays(-10);

        var result = await CreateService().CreateAsync(7, new CreateVaccinationRequest
        {
            VaccineName = "Rabies",
            VaccinatedAt = vaccinatedAt,
            ImmunityYears = 1,
            VaccinationType = HappyPet.Domain.Enums.VaccinationType.Mandatory,
            ManufacturerCountry = "საფრანგეთი"
        });

        Assert.Equal(vaccinatedAt.AddYears(1).AddMonths(-1), result.NextDueAt);
        Assert.Equal(1, result.ImmunityYears);
        Assert.Equal(HappyPet.Domain.Enums.VaccinationType.Mandatory, result.VaccinationType);
        Assert.Equal("საფრანგეთი", result.ManufacturerCountry);
        Assert.False(result.IsNextDueOverridden);
    }

    [Fact]
    public async Task CreateAsync_WithNextDueOverride_WhenNotAllowed_Throws()
    {
        _petRepository
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildPet());
        var vaccinatedAt = DateTime.UtcNow.AddDays(-10);
        var computed = vaccinatedAt.AddYears(1).AddMonths(-1);

        await Assert.ThrowsAsync<ArgumentException>(() => CreateService().CreateAsync(7, new CreateVaccinationRequest
        {
            VaccineName = "Rabies",
            VaccinatedAt = vaccinatedAt,
            ImmunityYears = 1,
            NextDueAt = computed.AddDays(7),
            OverrideNextDueDate = true
        }));
    }

    [Fact]
    public async Task CreateAsync_WithNextDueOverride_WhenAllowed_MarksOverride()
    {
        _petRepository
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildPet());
        var vaccinatedAt = DateTime.UtcNow.AddDays(-10);
        var overridden = vaccinatedAt.AddYears(1).AddMonths(-1).AddDays(7);

        var result = await CreateService().CreateAsync(7, new CreateVaccinationRequest
        {
            VaccineName = "Rabies",
            VaccinatedAt = vaccinatedAt,
            ImmunityYears = 1,
            NextDueAt = overridden,
            OverrideNextDueDate = true
        }, allowNextDueOverride: true);

        Assert.Equal(overridden, result.NextDueAt);
        Assert.True(result.IsNextDueOverridden);
    }

    [Fact]
    public async Task CreateAsync_WhenVaccinationIsOlderThanOneYear_Throws()
    {
        _petRepository
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildPet());

        await Assert.ThrowsAsync<ArgumentException>(() => CreateService().CreateAsync(7, new CreateVaccinationRequest
        {
            VaccineName = "Rabies",
            VaccinatedAt = DateTime.UtcNow.AddYears(-1).AddDays(-1),
            ImmunityYears = 1
        }));
    }

    [Fact]
    public async Task CreateAsync_WhenPreviousVaccinationStillActive_Throws()
    {
        _petRepository
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildPet());
        _vaccinationRepository
            .Setup(r => r.GetByPetIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vaccination>
            {
                new()
                {
                    Id = 1,
                    PetId = 7,
                    VaccineName = "Rabies",
                    VaccinatedAt = DateTime.UtcNow.AddMonths(-2),
                    NextDueAt = DateTime.UtcNow.AddMonths(10)
                }
            });

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService().CreateAsync(7, new CreateVaccinationRequest
        {
            VaccineName = "Bordetella",
            VaccinatedAt = DateTime.UtcNow.AddDays(-1),
            ImmunityYears = 1
        }));
    }

    [Fact]
    public async Task CreateAsync_WhenPreviousVaccinationStillActive_AndEarlyAllowed_Succeeds()
    {
        _petRepository
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildPet());
        _vaccinationRepository
            .Setup(r => r.GetByPetIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vaccination>
            {
                new()
                {
                    Id = 1,
                    PetId = 7,
                    VaccineName = "Rabies",
                    VaccinatedAt = DateTime.UtcNow.AddMonths(-2),
                    NextDueAt = DateTime.UtcNow.AddMonths(10)
                }
            });

        var result = await CreateService().CreateAsync(7, new CreateVaccinationRequest
        {
            VaccineName = "Bordetella",
            VaccinatedAt = DateTime.UtcNow.AddDays(-1),
            ImmunityYears = 1
        }, allowEarlyVaccination: true);

        Assert.Equal("Bordetella", result.VaccineName);
        _vaccinationRepository.Verify(
            r => r.AddAsync(It.IsAny<Vaccination>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenPetIsDeceased_Throws()
    {
        var pet = BuildPet();
        pet.IsDeceased = true;
        _petRepository
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pet);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().CreateAsync(7, BuildCreateRequest()));
    }

    private static CreateVaccinationRequest BuildCreateRequest()
    {
        return new CreateVaccinationRequest
        {
            VaccineName = "Rabies",
            VaccinatedAt = DateTime.UtcNow.AddDays(-10),
            NextDueAt = DateTime.UtcNow.AddDays(355)
        };
    }

    private static UpdateVaccinationRequest BuildUpdateRequest()
    {
        return new UpdateVaccinationRequest
        {
            VaccineName = "Rabies",
            VaccinatedAt = DateTime.UtcNow.AddDays(-10)
        };
    }

    private static Pet BuildPet()
    {
        return new Pet
        {
            Id = 7,
            OwnerId = 1,
            Name = "შერი",
            Species = PetSpecies.Dog,
            Gender = PetGender.Female,
            MicrochipNumber = "982000000000001"
        };
    }
}
