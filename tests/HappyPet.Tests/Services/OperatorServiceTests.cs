using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Services;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;
using Moq;

namespace HappyPet.Tests.Services;

public class OperatorServiceTests
{
    private readonly Mock<IMarketplaceRepository> _marketplaceRepository = new();
    private readonly Mock<IPetRepository> _petRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public OperatorServiceTests()
    {
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private OperatorService CreateService() => new(
        _marketplaceRepository.Object,
        _petRepository.Object,
        _userRepository.Object,
        _unitOfWork.Object);

    [Fact]
    public async Task MarkSyncedAsync_WithLinkedPetAndBuyer_UpdatesPetOwnerAndTransfer()
    {
        var transfer = BuildTransfer();
        var pet = BuildPet();
        var buyer = BuildBuyer();

        _marketplaceRepository
            .Setup(r => r.GetOwnershipTransferByIdAsync(transfer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);
        _petRepository
            .Setup(r => r.GetByIdAsync(pet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pet);
        _userRepository
            .Setup(r => r.GetByIdAsync(buyer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(buyer);

        var result = await CreateService().MarkSyncedAsync(
            transfer.Id,
            99,
            new MarkOwnershipTransferSyncedRequest
            {
                AirsReference = " AIRS-2026-0001 ",
                BuyerAddress = "თბილისი, რუსთაველის გამზირი 1",
                Notes = "AIRS-ში დადასტურებულია"
            });

        Assert.Equal(OwnershipTransferStatus.Synced, result.Status);
        Assert.Equal("AIRS-2026-0001", result.AirsReference);
        Assert.Equal(99, result.ProcessedByUserId);
        Assert.Equal(buyer.Id, pet.OwnerId);
        Assert.Equal("თბილისი, რუსთაველის გამზირი 1", result.BuyerAddress);
        Assert.Equal("AIRS-ში დადასტურებულია", transfer.OperatorNotes);

        _petRepository.Verify(r => r.Update(pet), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkSyncedAsync_WithLinkedPetAndNoBuyerAccount_Throws()
    {
        var transfer = BuildTransfer();
        transfer.BuyerUserId = null;
        var pet = BuildPet();

        _marketplaceRepository
            .Setup(r => r.GetOwnershipTransferByIdAsync(transfer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);
        _petRepository
            .Setup(r => r.GetByIdAsync(pet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pet);
        _userRepository
            .Setup(r => r.GetByPersonalIdAsync(transfer.BuyerPersonalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().MarkSyncedAsync(
                transfer.Id,
                99,
                new MarkOwnershipTransferSyncedRequest { AirsReference = "AIRS-1" }));

        Assert.Equal(OwnershipTransferStatus.PendingAirsSync, transfer.Status);
        Assert.Equal(1, pet.OwnerId);
        _petRepository.Verify(r => r.Update(It.IsAny<Pet>()), Times.Never);
    }

    [Fact]
    public async Task MarkSyncedAsync_WhenBuyerPersonalIdMatchesExistingUser_LinksBuyerAutomatically()
    {
        var transfer = BuildTransfer();
        transfer.BuyerUserId = null;
        var pet = BuildPet();
        var buyer = BuildBuyer();

        _marketplaceRepository
            .Setup(r => r.GetOwnershipTransferByIdAsync(transfer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);
        _petRepository
            .Setup(r => r.GetByIdAsync(pet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pet);
        _userRepository
            .Setup(r => r.GetByPersonalIdAsync(transfer.BuyerPersonalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(buyer);

        var result = await CreateService().MarkSyncedAsync(
            transfer.Id,
            99,
            new MarkOwnershipTransferSyncedRequest { AirsReference = "AIRS-2" });

        Assert.Equal(buyer.Id, result.BuyerUserId);
        Assert.Equal(buyer.Id, pet.OwnerId);
        Assert.Equal(buyer.PhoneNumber, result.BuyerPhone);
    }

    [Fact]
    public async Task MarkSyncedAsync_ForStandaloneListingWithoutBuyerAccount_Succeeds()
    {
        var transfer = BuildTransfer();
        transfer.PetId = null;
        transfer.BuyerUserId = null;

        _marketplaceRepository
            .Setup(r => r.GetOwnershipTransferByIdAsync(transfer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);
        _userRepository
            .Setup(r => r.GetByPersonalIdAsync(transfer.BuyerPersonalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await CreateService().MarkSyncedAsync(
            transfer.Id,
            99,
            new MarkOwnershipTransferSyncedRequest { AirsReference = "AIRS-3" });

        Assert.Equal(OwnershipTransferStatus.Synced, result.Status);
        Assert.Null(result.BuyerUserId);
        _petRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkSyncedAsync_WhenAlreadyProcessed_Throws()
    {
        var transfer = BuildTransfer();
        transfer.Status = OwnershipTransferStatus.Synced;

        _marketplaceRepository
            .Setup(r => r.GetOwnershipTransferByIdAsync(transfer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().MarkSyncedAsync(
                transfer.Id,
                99,
                new MarkOwnershipTransferSyncedRequest { AirsReference = "AIRS-4" }));
    }

    [Fact]
    public async Task MarkSyncedAsync_WhenSyncedBeforeTransfer_Throws()
    {
        var transfer = BuildTransfer();

        _marketplaceRepository
            .Setup(r => r.GetOwnershipTransferByIdAsync(transfer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);

        await Assert.ThrowsAsync<ArgumentException>(
            () => CreateService().MarkSyncedAsync(
                transfer.Id,
                99,
                new MarkOwnershipTransferSyncedRequest
                {
                    AirsReference = "AIRS-5",
                    SyncedAt = transfer.TransferredAt.AddMinutes(-1)
                }));
    }

    [Fact]
    public async Task RejectAsync_SetsRejectedStatusAndReason()
    {
        var transfer = BuildTransfer();

        _marketplaceRepository
            .Setup(r => r.GetOwnershipTransferByIdAsync(transfer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);

        var result = await CreateService().RejectAsync(
            transfer.Id,
            99,
            new RejectOwnershipTransferRequest
            {
                Reason = "მყიდველის პირადი ნომერი არ დადასტურდა",
                Notes = "საჭიროა ხელახალი განცხადება"
            });

        Assert.Equal(OwnershipTransferStatus.Rejected, result.Status);
        Assert.Equal("მყიდველის პირადი ნომერი არ დადასტურდა", result.RejectionReason);
        Assert.Equal(99, result.ProcessedByUserId);
        Assert.NotNull(result.ProcessedAt);
    }

    [Fact]
    public async Task GetDashboardAsync_MapsStats()
    {
        _marketplaceRepository
            .Setup(r => r.GetOwnershipTransferStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OwnershipTransferStats
            {
                PendingCount = 3,
                SyncedCount = 5,
                RejectedCount = 1,
                TotalCount = 9
            });

        var result = await CreateService().GetDashboardAsync();

        Assert.Equal(3, result.PendingTransfers);
        Assert.Equal(5, result.SyncedTransfers);
        Assert.Equal(1, result.RejectedTransfers);
        Assert.Equal(9, result.TotalTransfers);
    }

    private static OwnershipTransfer BuildTransfer()
    {
        return new OwnershipTransfer
        {
            Id = 50,
            ListingId = 5,
            PetId = 10,
            MicrochipNumber = "982000000000001",
            SellerId = 1,
            BuyerUserId = 2,
            BuyerFullName = "ნინო ბერიძე",
            BuyerPersonalId = "01000000002",
            BuyerPhone = "+995555000000",
            TransferType = OwnershipTransferType.Sale,
            Price = 150,
            Status = OwnershipTransferStatus.PendingAirsSync,
            TransferredAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            Listing = new MarketplaceListing
            {
                Id = 5,
                SellerId = 1,
                PetId = 10,
                PetName = "შერი",
                Species = PetSpecies.Dog,
                Gender = PetGender.Female,
                BirthDate = DateTime.UtcNow.Date.AddDays(-100),
                ListingType = ListingType.Sale,
                Price = 150,
                City = "თბილისი",
                ContactPhone = "+995555000000",
                Status = ListingStatus.Completed,
                ExpiresAt = DateTime.UtcNow.AddDays(10),
                CompletedAt = DateTime.UtcNow.AddDays(-1),
                Seller = new User
                {
                    Id = 1,
                    FirstName = "გიორგი",
                    LastName = "ჯგარკავა",
                    PersonalId = "01000000001",
                    Email = "seller@example.com",
                    Password = "hash",
                    PhoneNumber = "+995555111111"
                }
            }
        };
    }

    private static Pet BuildPet()
    {
        return new Pet
        {
            Id = 10,
            OwnerId = 1,
            Name = "შერი",
            Species = PetSpecies.Dog,
            Gender = PetGender.Female,
            BirthDate = DateTime.UtcNow.Date.AddDays(-100),
            MicrochipNumber = "982000000000001",
            CreatedAt = DateTime.UtcNow.AddDays(-100)
        };
    }

    private static User BuildBuyer()
    {
        return new User
        {
            Id = 2,
            PersonalId = "01000000002",
            FirstName = "ნინო",
            LastName = "ბერიძე",
            Email = "buyer@example.com",
            Password = "hash",
            PhoneNumber = "+995555000000",
            Address = "თბილისი"
        };
    }
}
