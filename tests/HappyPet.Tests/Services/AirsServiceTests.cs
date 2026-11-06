using HappyPet.Application.Dtos.Airs;
using HappyPet.Application.Exceptions;
using HappyPet.Application.Interfaces.Airs;
using HappyPet.Application.Services;
using Moq;

namespace HappyPet.Tests.Services;

public class AirsServiceTests
{
    private readonly Mock<IAirsClient> _client = new();

    private AirsService CreateService() => new(_client.Object);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetAnimalByMicrochipAsync_WhenMicrochipIsBlank_ThrowsNotFound(string microchip)
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<AirsAnimalNotFoundException>(
            () => service.GetAnimalByMicrochipAsync(microchip));

        Assert.Equal("AIRS_MICROCHIP_NOT_FOUND", exception.Code);
        _client.Verify(
            c => c.GetAnimalByMicrochipAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAnimalByMicrochipAsync_WhenClientReturnsNull_ThrowsNotFound()
    {
        _client
            .Setup(c => c.GetAnimalByMicrochipAsync("982000000000001", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AirsAnimalDetails?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<AirsAnimalNotFoundException>(
            () => service.GetAnimalByMicrochipAsync("982000000000001"));
    }

    [Fact]
    public async Task EnsureMicrochipBelongsToOwnerAsync_WhenOwnerMatches_DoesNotThrow()
    {
        _client
            .Setup(c => c.GetAnimalByMicrochipAsync("982000000000001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildDetails("01000000001"));

        var service = CreateService();

        await service.EnsureMicrochipBelongsToOwnerAsync("982000000000001", "01000000001");
    }

    [Fact]
    public async Task EnsureMicrochipBelongsToOwnerAsync_WhenOwnerDiffers_ThrowsOwnerMismatch()
    {
        _client
            .Setup(c => c.GetAnimalByMicrochipAsync("982000000000001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildDetails("01000000002"));

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<AirsOwnerMismatchException>(
            () => service.EnsureMicrochipBelongsToOwnerAsync("982000000000001", "01000000001"));

        Assert.Equal("AIRS_OWNER_MISMATCH", exception.Code);
    }

    [Fact]
    public async Task EnsureMicrochipBelongsToOwnerAsync_WhenAirsOwnerIsMissing_ThrowsOwnerMismatch()
    {
        _client
            .Setup(c => c.GetAnimalByMicrochipAsync("982000000000001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildDetails(null));

        var service = CreateService();

        await Assert.ThrowsAsync<AirsOwnerMismatchException>(
            () => service.EnsureMicrochipBelongsToOwnerAsync("982000000000001", "01000000001"));
    }

    [Fact]
    public async Task EnsureMicrochipBelongsToOwnerAsync_WhenOwnerPersonalIdIsBlank_ThrowsInvalidOperation()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.EnsureMicrochipBelongsToOwnerAsync("982000000000001", " "));

        _client.Verify(
            c => c.GetAnimalByMicrochipAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EnsureMicrochipBelongsToOwnerAsync_WhenClientThrowsUnavailable_Propagates()
    {
        _client
            .Setup(c => c.GetAnimalByMicrochipAsync("982000000000001", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AirsUnavailableException("AIRS მიუწვდომელია."));

        var service = CreateService();

        await Assert.ThrowsAsync<AirsUnavailableException>(
            () => service.EnsureMicrochipBelongsToOwnerAsync("982000000000001", "01000000001"));
    }

    private static AirsAnimalDetails BuildDetails(string? ownerPersonalId)
    {
        return new AirsAnimalDetails
        {
            Animal = new AirsAnimal
            {
                MicrochipNumber = "982000000000001",
                Owner = ownerPersonalId == null
                    ? null
                    : new AirsOwner { PersonalId = ownerPersonalId }
            }
        };
    }
}
