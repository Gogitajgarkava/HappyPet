using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Storage;
using HappyPet.Application.Services;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;
using Moq;

namespace HappyPet.Tests.Services;

public class MarketplaceServiceTests
{
    private readonly Mock<IMarketplaceRepository> _marketplaceRepository = new();
    private readonly Mock<IPetRepository> _petRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();

    public MarketplaceServiceTests()
    {
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _fileStorage
            .Setup(f => f.GetUrl(It.IsAny<string>()))
            .Returns("https://cdn.example.com/photo.jpg");
    }

    private MarketplaceService CreateService() => new(
        _marketplaceRepository.Object,
        _petRepository.Object,
        _userRepository.Object,
        _unitOfWork.Object,
        _fileStorage.Object);

    [Fact]
    public async Task CreateAsync_Giveaway_WithZeroPrice_NormalizesPriceAndSetsThirtyDayExpiry()
    {
        MarketplaceListing? created = null;
        _marketplaceRepository
            .Setup(r => r.AddAsync(It.IsAny<MarketplaceListing>(), It.IsAny<CancellationToken>()))
            .Callback<MarketplaceListing, CancellationToken>((listing, _) => created = listing)
            .Returns(Task.CompletedTask);

        var request = BuildCreateRequest();
        request.ListingType = ListingType.Giveaway;
        request.Price = 0;

        var before = DateTime.UtcNow;
        var result = await CreateService().CreateAsync(1, request);
        var after = DateTime.UtcNow;

        Assert.NotNull(created);
        Assert.Null(result.Price);
        Assert.Equal(ListingStatus.Active, result.Status);
        Assert.InRange(created!.ExpiresAt, before.AddDays(30), after.AddDays(30));
    }

    [Fact]
    public async Task CreateAsync_SaleWithoutPrice_ThrowsArgument()
    {
        var request = BuildCreateRequest();
        request.ListingType = ListingType.Sale;
        request.Price = null;

        await Assert.ThrowsAsync<ArgumentException>(
            () => CreateService().CreateAsync(1, request));
    }

    [Fact]
    public async Task CreateAsync_UnderEightWeeks_ThrowsArgument()
    {
        var request = BuildCreateRequest();
        request.BirthDate = DateTime.UtcNow.Date.AddDays(-30);

        await Assert.ThrowsAsync<ArgumentException>(
            () => CreateService().CreateAsync(1, request));
    }

    [Fact]
    public async Task CreateAsync_WithLinkedPetNotOwned_ThrowsUnauthorized()
    {
        _petRepository
            .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Pet
            {
                Id = 10,
                OwnerId = 2,
                Name = "შერი",
                BirthDate = DateTime.UtcNow.Date.AddDays(-100)
            });

        var request = BuildCreateRequest();
        request.PetId = 10;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateService().CreateAsync(1, request));
    }

    [Fact]
    public async Task CreateAsync_WithLinkedPetHavingActiveListing_ThrowsInvalidOperation()
    {
        _petRepository
            .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Pet
            {
                Id = 10,
                OwnerId = 1,
                Name = "შერი",
                BirthDate = DateTime.UtcNow.Date.AddDays(-100)
            });
        _marketplaceRepository
            .Setup(r => r.HasActiveListingForPetAsync(10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = BuildCreateRequest();
        request.PetId = 10;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().CreateAsync(1, request));
    }

    [Fact]
    public async Task CreateAsync_WithLinkedPet_CopiesPetMicrochip()
    {
        _petRepository
            .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Pet
            {
                Id = 10,
                OwnerId = 1,
                Name = "შერი",
                BirthDate = DateTime.UtcNow.Date.AddDays(-100),
                MicrochipNumber = "982000000000001"
            });
        _marketplaceRepository
            .Setup(r => r.HasActiveListingForPetAsync(10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _marketplaceRepository
            .Setup(r => r.AddAsync(It.IsAny<MarketplaceListing>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = BuildCreateRequest();
        request.PetId = 10;
        request.MicrochipNumber = null;

        var result = await CreateService().CreateAsync(1, request);

        Assert.Equal("982000000000001", result.MicrochipNumber);
    }

    [Fact]
    public async Task CompleteAsync_CreatesPendingAirsTransferAndCompletesListing()
    {
        var listing = BuildListing();
        OwnershipTransfer? savedTransfer = null;

        _marketplaceRepository
            .Setup(r => r.GetByIdAsync(listing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listing);
        _marketplaceRepository
            .Setup(r => r.GetPhotosByListingIdAsync(listing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MarketplaceListingPhoto>());
        _marketplaceRepository
            .Setup(r => r.AddOwnershipTransferAsync(It.IsAny<OwnershipTransfer>(), It.IsAny<CancellationToken>()))
            .Callback<OwnershipTransfer, CancellationToken>((transfer, _) => savedTransfer = transfer)
            .Returns(Task.CompletedTask);
        _userRepository
            .Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Id = 2,
                PersonalId = "01000000002",
                FirstName = "ნინო",
                LastName = "ბერიძე",
                PhoneNumber = "+995555000000"
            });

        var request = new CompleteMarketplaceListingRequest
        {
            BuyerUserId = 2,
            BuyerFullName = "ნინო ბერიძე",
            BuyerPersonalId = "01000000002"
        };

        var result = await CreateService().CompleteAsync(listing.Id, 1, request);

        Assert.Equal(ListingStatus.Completed, listing.Status);
        Assert.NotNull(savedTransfer);
        Assert.Equal(OwnershipTransferStatus.PendingAirsSync, savedTransfer!.Status);
        Assert.Equal(OwnershipTransferType.Sale, savedTransfer.TransferType);
        Assert.Equal(150m, savedTransfer.Price);
        Assert.Equal("+995555000000", savedTransfer.BuyerPhone);
        Assert.Equal(OwnershipTransferStatus.PendingAirsSync, result.OwnershipTransfer.Status);
    }

    [Fact]
    public async Task CompleteAsync_WhenExpired_MarksExpiredAndThrows()
    {
        var listing = BuildListing();
        listing.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        _marketplaceRepository
            .Setup(r => r.GetByIdAsync(listing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listing);

        var request = new CompleteMarketplaceListingRequest
        {
            BuyerFullName = "ნინო ბერიძე",
            BuyerPersonalId = "01000000002"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().CompleteAsync(listing.Id, 1, request));

        Assert.Equal(ListingStatus.Expired, listing.Status);
    }

    [Fact]
    public async Task BlockAsync_SetsBlockedStatusAndReason()
    {
        var listing = BuildListing();
        _marketplaceRepository
            .Setup(r => r.GetByIdAsync(listing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listing);

        var result = await CreateService().BlockAsync(
            listing.Id,
            99,
            new BlockMarketplaceListingRequest { Reason = "საეჭვო განცხადება" });

        Assert.Equal(ListingStatus.Blocked, result.Status);
        Assert.Equal("საეჭვო განცხადება", result.BlockReason);
        Assert.Equal(99, listing.BlockedByUserId);
    }

    [Fact]
    public async Task UpdateAsync_WhenListingIsNotActive_ThrowsInvalidOperation()
    {
        var listing = BuildListing();
        listing.Status = ListingStatus.Completed;
        _marketplaceRepository
            .Setup(r => r.GetByIdAsync(listing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listing);

        var request = new UpdateMarketplaceListingRequest
        {
            PetName = "შერი",
            Species = PetSpecies.Dog,
            Gender = PetGender.Female,
            BirthDate = DateTime.UtcNow.Date.AddDays(-100),
            ListingType = ListingType.Giveaway,
            City = "თბილისი",
            ContactPhone = "+995555000000"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().UpdateAsync(listing.Id, request));
    }

    private static CreateMarketplaceListingRequest BuildCreateRequest()
    {
        return new CreateMarketplaceListingRequest
        {
            PetName = "შერი",
            Species = PetSpecies.Dog,
            Breed = "ლაბრადორი",
            Gender = PetGender.Female,
            BirthDate = DateTime.UtcNow.Date.AddDays(-70),
            ListingType = ListingType.Sale,
            Price = 150,
            City = "თბილისი",
            ContactPhone = "+995555000000",
            Description = "მეგობრული ლეკვი"
        };
    }

    private static MarketplaceListing BuildListing()
    {
        return new MarketplaceListing
        {
            Id = 5,
            SellerId = 1,
            PetName = "შერი",
            Species = PetSpecies.Dog,
            Gender = PetGender.Female,
            BirthDate = DateTime.UtcNow.Date.AddDays(-100),
            ListingType = ListingType.Sale,
            Price = 150,
            City = "თბილისი",
            ContactPhone = "+995555000000",
            Status = ListingStatus.Active,
            ExpiresAt = DateTime.UtcNow.AddDays(10),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            Photos = new List<MarketplaceListingPhoto>()
        };
    }
}
