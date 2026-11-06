using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Application.Interfaces.Storage;
using HappyPet.Application.Services;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;
using Moq;

namespace HappyPet.Tests.Services;

public class PetServiceTests
{
    private readonly Mock<IPetRepository> _petRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IAirsService> _airsService = new();

    public PetServiceTests()
    {
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private PetService CreateService() => new(
        _petRepository.Object,
        _unitOfWork.Object,
        _fileStorage.Object,
        _userRepository.Object,
        _airsService.Object);

    [Fact]
    public async Task CreateAsync_WithEmptyMicrochip_SkipsAirsValidation()
    {
        var request = BuildCreateRequest(microchip: "   ");

        var result = await CreateService().CreateAsync(1, request);

        Assert.Equal("შერი", result.Name);
        Assert.Equal(string.Empty, result.MicrochipNumber);
        Assert.Equal(PetSpecies.Dog, result.Species);
        Assert.Equal(PetGender.Female, result.Gender);

        _airsService.Verify(
            a => a.EnsureMicrochipBelongsToOwnerAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _petRepository.Verify(
            r => r.AddAsync(It.IsAny<Pet>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateMicrochip_ThrowsBeforeAirsLookup()
    {
        _petRepository
            .Setup(r => r.ExistsByMicrochipNumberAsync("982000000000001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = BuildCreateRequest(microchip: "982000000000001");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().CreateAsync(1, request));

        Assert.Contains("უკვე რეგისტრირებულია", exception.Message);
        _airsService.Verify(
            a => a.EnsureMicrochipBelongsToOwnerAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _petRepository.Verify(
            r => r.AddAsync(It.IsAny<Pet>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithValidMicrochip_ValidatesAgainstAirs()
    {
        _petRepository
            .Setup(r => r.ExistsByMicrochipNumberAsync("982000000000001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = 1, PersonalId = "01000000001" });

        var request = BuildCreateRequest(microchip: "982000000000001");

        var result = await CreateService().CreateAsync(1, request);

        Assert.Equal("982000000000001", result.MicrochipNumber);
        _airsService.Verify(
            a => a.EnsureMicrochipBelongsToOwnerAsync(
                "982000000000001",
                "01000000001",
                It.IsAny<CancellationToken>()),
            Times.Once);
        _petRepository.Verify(
            r => r.AddAsync(It.IsAny<Pet>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenUserIsMissing_ThrowsInvalidOperation()
    {
        _petRepository
            .Setup(r => r.ExistsByMicrochipNumberAsync("982000000000001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var request = BuildCreateRequest(microchip: "982000000000001");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().CreateAsync(1, request));

        _airsService.Verify(
            a => a.EnsureMicrochipBelongsToOwnerAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithUnchangedMicrochip_SkipsAirsValidation()
    {
        var pet = BuildPet(microchip: "982000000000001");
        _petRepository
            .Setup(r => r.GetByIdAsync(pet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pet);

        var request = BuildUpdateRequest(microchip: "982000000000001", name: "შერი II");

        var result = await CreateService().UpdateAsync(pet.Id, request);

        Assert.Equal("შერი II", result.Name);
        _airsService.Verify(
            a => a.EnsureMicrochipBelongsToOwnerAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithChangedMicrochip_ValidatesAgainstAirs()
    {
        var pet = BuildPet(microchip: "982000000000001");
        _petRepository
            .Setup(r => r.GetByIdAsync(pet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pet);
        _petRepository
            .Setup(r => r.ExistsByMicrochipNumberAsync("982000000000002", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepository
            .Setup(r => r.GetByIdAsync(pet.OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = pet.OwnerId, PersonalId = "01000000001" });

        var request = BuildUpdateRequest(microchip: "982000000000002");

        var result = await CreateService().UpdateAsync(pet.Id, request);

        Assert.Equal("982000000000002", result.MicrochipNumber);
        _airsService.Verify(
            a => a.EnsureMicrochipBelongsToOwnerAsync(
                "982000000000002",
                "01000000001",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenPetDoesNotExist_ThrowsKeyNotFound()
    {
        _petRepository
            .Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pet?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CreateService().UpdateAsync(42, BuildUpdateRequest()));
    }

    [Fact]
    public async Task GetLostAsync_ReturnsPagedEnvelopeAndClampsPageSize()
    {
        _petRepository
            .Setup(r => r.GetLostPagedAsync(0, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Pet> { BuildPet(), BuildPet() }, 42));

        var result = await CreateService().GetLostAsync(page: 0, pageSize: 500);

        Assert.Equal(1, result.Page);
        Assert.Equal(100, result.PageSize);
        Assert.Equal(42, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task AddPhotoAsync_RejectsUnsupportedContentType()
    {
        var pet = BuildPet();
        _petRepository
            .Setup(r => r.GetByIdAsync(pet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pet);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().AddPhotoAsync(pet.Id, stream, stream.Length, "application/pdf"));

        _fileStorage.Verify(
            f => f.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static CreatePetRequest BuildCreateRequest(string microchip = "")
    {
        return new CreatePetRequest
        {
            Name = "შერი",
            Species = PetSpecies.Dog,
            Breed = "German Shepherd",
            MicrochipNumber = microchip,
            Gender = PetGender.Female,
            Color = "თეთრი"
        };
    }

    private static UpdatePetRequest BuildUpdateRequest(
        string microchip = "982000000000001",
        string name = "შერი")
    {
        return new UpdatePetRequest
        {
            Name = name,
            Species = PetSpecies.Dog,
            Breed = "German Shepherd",
            MicrochipNumber = microchip,
            Gender = PetGender.Female,
            Color = "თეთრი"
        };
    }

    private static Pet BuildPet(string microchip = "982000000000001")
    {
        return new Pet
        {
            Id = 7,
            OwnerId = 1,
            Name = "შერი",
            Species = PetSpecies.Dog,
            Breed = "German Shepherd",
            MicrochipNumber = microchip,
            Gender = PetGender.Female,
            Color = "თეთრი",
            CreatedAt = DateTime.UtcNow
        };
    }
}
