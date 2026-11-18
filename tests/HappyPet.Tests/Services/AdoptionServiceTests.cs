using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Storage;
using HappyPet.Application.Services;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;
using Moq;

namespace HappyPet.Tests.Services;

public class AdoptionServiceTests
{
    private readonly Mock<IAdoptionRepository> _adoptionRepository = new();
    private readonly Mock<IShelterRepository> _shelterRepository = new();
    private readonly Mock<IPetRepository> _petRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();

    public AdoptionServiceTests()
    {
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _fileStorage
            .Setup(f => f.GetUrl(It.IsAny<string>()))
            .Returns("https://cdn.example.com/photo.jpg");
    }

    private AdoptionService CreateService() => new(
        _adoptionRepository.Object,
        _shelterRepository.Object,
        _petRepository.Object,
        _unitOfWork.Object,
        _fileStorage.Object);

    private static ShelterProfile CreateShelter(
        int userId = 10,
        bool verified = true,
        bool accepting = true)
    {
        var user = new User
        {
            Id = userId,
            FirstName = "Demo",
            LastName = "Shelter",
            PhoneNumber = "+995555000000",
            Email = "shelter@example.com"
        };

        var profile = new ShelterProfile
        {
            Id = 100,
            UserId = userId,
            OrganizationName = "Happy Shelter",
            City = "Tbilisi",
            PhoneNumber = "+995555000000",
            Email = "shelter@example.com",
            IsVerified = verified,
            IsAcceptingApplications = accepting,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            User = user
        };

        user.ShelterProfile = profile;
        return profile;
    }

    private static Pet CreatePet(int ownerId = 10)
    {
        var pet = new Pet
        {
            Id = 2,
            OwnerId = ownerId,
            Name = "Bella",
            Species = PetSpecies.Dog,
            Breed = "Mixed",
            Gender = PetGender.Female,
            BirthDate = DateTime.UtcNow.AddYears(-2),
            MicrochipNumber = string.Empty,
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        pet.Photos.Add(new PetPhoto
        {
            Id = 3,
            PetId = pet.Id,
            Key = "demo/pets/bella.jpg",
            ContentType = "image/jpeg",
            SizeBytes = 1024,
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow.AddDays(-19)
        });

        return pet;
    }

    private static AdoptionListing CreateListing(
        ShelterProfile shelter,
        Pet pet,
        AdoptionListingStatus status = AdoptionListingStatus.Active)
    {
        return new AdoptionListing
        {
            Id = 55,
            ShelterId = shelter.UserId,
            PetId = pet.Id,
            Pet = pet,
            Shelter = shelter.User,
            Description = "Friendly dog",
            City = "Tbilisi",
            Status = status,
            PublishedAt = DateTime.UtcNow.AddDays(-5),
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };
    }

    private static AdoptionApplication CreateApplication(
        AdoptionListing listing,
        int applicantUserId = 20,
        AdoptionApplicationStatus status = AdoptionApplicationStatus.Pending)
    {
        var applicant = new User
        {
            Id = applicantUserId,
            FirstName = "Nino",
            LastName = "Beridze",
            PhoneNumber = "+995555111111",
            Email = "nino@example.com"
        };

        return new AdoptionApplication
        {
            Id = 77,
            ListingId = listing.Id,
            Listing = listing,
            ApplicantUserId = applicantUserId,
            Applicant = applicant,
            FullName = "Nino Beridze",
            Email = "nino@example.com",
            Phone = "+995555111111",
            Status = status,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
    }

    [Fact]
    public async Task CreateListingAsync_WhenShelterVerifiedAndOwnsPet_CreatesActiveListing()
    {
        var shelter = CreateShelter();
        var pet = CreatePet();

        _shelterRepository
            .Setup(r => r.GetByUserIdAsync(shelter.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shelter);
        _petRepository
            .Setup(r => r.GetByIdAsync(pet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pet);
        _adoptionRepository
            .Setup(r => r.HasActiveListingForPetAsync(
                pet.Id,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await CreateService().CreateListingAsync(
            shelter.UserId,
            new CreateAdoptionListingRequest
            {
                PetId = pet.Id,
                Description = "  Friendly dog  ",
                City = "  Tbilisi  ",
                AdoptionFee = 50,
                IsUrgent = true
            });

        Assert.Equal(AdoptionListingStatus.Active, result.Status);
        Assert.Equal(pet.Id, result.PetId);
        Assert.Equal("Bella", result.PetName);
        Assert.Equal("Happy Shelter", result.Shelter.OrganizationName);
        Assert.Equal("Friendly dog", result.Description);
        Assert.True(result.IsUrgent);
        Assert.Single(result.Photos);
        _adoptionRepository.Verify(
            r => r.AddListingAsync(
                It.Is<AdoptionListing>(l =>
                    l.PetId == pet.Id
                    && l.ShelterId == shelter.UserId
                    && l.Status == AdoptionListingStatus.Active),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateListingAsync_WhenShelterUnverified_Throws()
    {
        var shelter = CreateShelter(verified: false);

        _shelterRepository
            .Setup(r => r.GetByUserIdAsync(shelter.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shelter);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().CreateListingAsync(
                shelter.UserId,
                new CreateAdoptionListingRequest
                {
                    PetId = 2,
                    City = "Tbilisi"
                }));
    }

    [Fact]
    public async Task CreateListingAsync_WhenPetBelongsToAnotherUser_Throws()
    {
        var shelter = CreateShelter();
        var pet = CreatePet(ownerId: 99);

        _shelterRepository
            .Setup(r => r.GetByUserIdAsync(shelter.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shelter);
        _petRepository
            .Setup(r => r.GetByIdAsync(pet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pet);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().CreateListingAsync(
                shelter.UserId,
                new CreateAdoptionListingRequest
                {
                    PetId = pet.Id,
                    City = "Tbilisi"
                }));
    }

    [Fact]
    public async Task ApplyAsync_WhenListingActiveAndAccepting_CreatesPendingApplication()
    {
        var shelter = CreateShelter();
        var pet = CreatePet();
        var listing = CreateListing(shelter, pet);

        _adoptionRepository
            .Setup(r => r.GetListingByIdAsync(listing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listing);
        _adoptionRepository
            .Setup(r => r.GetApplicationByListingAndApplicantAsync(
                listing.Id,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdoptionApplication?)null);

        var result = await CreateService().ApplyAsync(
            listing.Id,
            20,
            new CreateAdoptionApplicationRequest
            {
                FullName = "  Nino Beridze  ",
                Email = "NINO@EXAMPLE.COM",
                Phone = "+995555111111",
                Message = "  We have a garden.  "
            });

        Assert.Equal(AdoptionApplicationStatus.Pending, result.Status);
        Assert.Equal("Nino Beridze", result.FullName);
        Assert.Equal("nino@example.com", result.Email);
        Assert.Equal("We have a garden.", result.Message);
        _adoptionRepository.Verify(
            r => r.AddApplicationAsync(
                It.Is<AdoptionApplication>(a =>
                    a.ListingId == listing.Id
                    && a.ApplicantUserId == 20
                    && a.Status == AdoptionApplicationStatus.Pending),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ApplyAsync_WhenApplicantIsShelter_Throws()
    {
        var shelter = CreateShelter();
        var pet = CreatePet();
        var listing = CreateListing(shelter, pet);

        _adoptionRepository
            .Setup(r => r.GetListingByIdAsync(listing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listing);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().ApplyAsync(
                listing.Id,
                shelter.UserId,
                new CreateAdoptionApplicationRequest
                {
                    FullName = "Nino Beridze",
                    Email = "nino@example.com",
                    Phone = "+995555111111"
                }));
    }

    [Fact]
    public async Task ApplyAsync_WhenShelterNotAccepting_Throws()
    {
        var shelter = CreateShelter(accepting: false);
        var pet = CreatePet();
        var listing = CreateListing(shelter, pet);

        _adoptionRepository
            .Setup(r => r.GetListingByIdAsync(listing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listing);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().ApplyAsync(
                listing.Id,
                20,
                new CreateAdoptionApplicationRequest
                {
                    FullName = "Nino Beridze",
                    Email = "nino@example.com",
                    Phone = "+995555111111"
                }));
    }

    [Fact]
    public async Task ApplyAsync_WhenAlreadyApplied_Throws()
    {
        var shelter = CreateShelter();
        var pet = CreatePet();
        var listing = CreateListing(shelter, pet);
        var existing = CreateApplication(listing);

        _adoptionRepository
            .Setup(r => r.GetListingByIdAsync(listing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listing);
        _adoptionRepository
            .Setup(r => r.GetApplicationByListingAndApplicantAsync(
                listing.Id,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().ApplyAsync(
                listing.Id,
                20,
                new CreateAdoptionApplicationRequest
                {
                    FullName = "Nino Beridze",
                    Email = "nino@example.com",
                    Phone = "+995555111111"
                }));
    }

    [Fact]
    public async Task ApproveApplicationAsync_WhenPending_ApprovesApplication()
    {
        var shelter = CreateShelter();
        var pet = CreatePet();
        var listing = CreateListing(shelter, pet);
        var application = CreateApplication(listing);

        _adoptionRepository
            .Setup(r => r.GetApplicationByIdAsync(application.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var result = await CreateService().ApproveApplicationAsync(
            application.Id,
            shelter.UserId,
            new ReviewAdoptionApplicationRequest { Note = "  Great fit  " });

        Assert.Equal(AdoptionApplicationStatus.Approved, result.Status);
        Assert.Equal(shelter.UserId, application.ReviewedByUserId);
        Assert.NotNull(application.ReviewedAt);
        Assert.Equal("Great fit", application.DecisionNote);
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ApproveApplicationAsync_WhenDifferentShelter_Throws()
    {
        var shelter = CreateShelter();
        var pet = CreatePet();
        var listing = CreateListing(shelter, pet);
        var application = CreateApplication(listing);

        _adoptionRepository
            .Setup(r => r.GetApplicationByIdAsync(application.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateService().ApproveApplicationAsync(
                application.Id,
                999,
                new ReviewAdoptionApplicationRequest()));
    }

    [Fact]
    public async Task CompleteApplicationAsync_WhenApproved_CompletesAdoptionAndTransfersPet()
    {
        var shelter = CreateShelter();
        var pet = CreatePet();
        var listing = CreateListing(shelter, pet);
        var application = CreateApplication(listing, status: AdoptionApplicationStatus.Approved);
        var other = CreateApplication(listing, applicantUserId: 21);
        other.Id = 78;

        _adoptionRepository
            .Setup(r => r.GetApplicationByIdAsync(application.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);
        _petRepository
            .Setup(r => r.GetByIdAsync(pet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pet);
        _adoptionRepository
            .Setup(r => r.GetApplicationsByListingAsync(listing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AdoptionApplication> { application, other });

        var result = await CreateService().CompleteApplicationAsync(
            application.Id,
            shelter.UserId,
            new ReviewAdoptionApplicationRequest { Note = "Adopted" });

        Assert.Equal(AdoptionApplicationStatus.Completed, result.Status);
        Assert.Equal(AdoptionListingStatus.Adopted, listing.Status);
        Assert.NotNull(listing.ClosedAt);
        Assert.Equal(application.ApplicantUserId, pet.OwnerId);
        Assert.Equal(AdoptionApplicationStatus.Rejected, other.Status);
        Assert.Equal("სხვა განაცხადი დაკმაყოფილდა.", other.DecisionNote);
        _petRepository.Verify(r => r.Update(pet), Times.Once);
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WithdrawListingAsync_WhenActive_RejectsOpenApplications()
    {
        var shelter = CreateShelter();
        var pet = CreatePet();
        var listing = CreateListing(shelter, pet);
        var pending = CreateApplication(listing);
        var approved = CreateApplication(listing, applicantUserId: 21, status: AdoptionApplicationStatus.Approved);
        approved.Id = 78;

        _adoptionRepository
            .Setup(r => r.GetListingByIdAsync(listing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listing);
        _adoptionRepository
            .Setup(r => r.GetApplicationsByListingAsync(listing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AdoptionApplication> { pending, approved });

        var result = await CreateService().WithdrawListingAsync(
            listing.Id,
            shelter.UserId);

        Assert.Equal(AdoptionListingStatus.Withdrawn, result.Status);
        Assert.NotNull(listing.ClosedAt);
        Assert.Equal(AdoptionApplicationStatus.Rejected, pending.Status);
        Assert.Equal(AdoptionApplicationStatus.Rejected, approved.Status);
    }

    [Fact]
    public async Task GetPublicListingAsync_WhenShelterUnverified_ReturnsNull()
    {
        var shelter = CreateShelter(verified: false);
        var pet = CreatePet();
        var listing = CreateListing(shelter, pet);

        _adoptionRepository
            .Setup(r => r.GetListingByIdAsync(listing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listing);

        var result = await CreateService().GetPublicListingAsync(listing.Id);

        Assert.Null(result);
    }
}
