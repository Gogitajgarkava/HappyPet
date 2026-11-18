using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Services;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;
using Moq;

namespace HappyPet.Tests.Services;

public class ShelterServiceTests
{
    private readonly Mock<IShelterRepository> _shelterRepository = new();
    private readonly Mock<IAdoptionRepository> _adoptionRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public ShelterServiceTests()
    {
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private ShelterService CreateService() => new(
        _shelterRepository.Object,
        _adoptionRepository.Object,
        _userRepository.Object,
        _unitOfWork.Object);

    private static UpdateShelterProfileRequest ValidRequest(string organizationName = "Happy Tails") => new()
    {
        OrganizationName = organizationName,
        RegistrationNumber = "405123456",
        Description = "  A small shelter.  ",
        City = "  Tbilisi  ",
        Address = "  Rustaveli 1  ",
        PhoneNumber = "+995 555 123 456",
        Email = "shelter@example.com",
        Website = "https://example.com",
        IsAcceptingApplications = null
    };

    [Fact]
    public async Task UpsertProfileAsync_WhenProfileDoesNotExist_CreatesUnverifiedProfile()
    {
        _shelterRepository
            .Setup(r => r.GetByUserIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShelterProfile?)null);
        _shelterRepository
            .Setup(r => r.ExistsByRegistrationNumberAsync(
                "405123456",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        ShelterProfile? added = null;
        _shelterRepository
            .Setup(r => r.AddAsync(It.IsAny<ShelterProfile>(), It.IsAny<CancellationToken>()))
            .Callback<ShelterProfile, CancellationToken>((profile, _) => added = profile)
            .Returns(Task.CompletedTask);

        var result = await CreateService().UpsertProfileAsync(7, ValidRequest());

        Assert.NotNull(added);
        Assert.Equal(7, added!.UserId);
        Assert.Equal("Happy Tails", added.OrganizationName);
        Assert.Equal("Tbilisi", added.City);
        Assert.Equal("Rustaveli 1", added.Address);
        Assert.Equal("A small shelter.", added.Description);
        Assert.False(added.IsVerified);
        Assert.True(added.IsAcceptingApplications);
        Assert.False(result.IsVerified);
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpsertProfileAsync_WhenLegalIdentityChanges_ResetsVerification()
    {
        var existing = new ShelterProfile
        {
            Id = 12,
            UserId = 7,
            OrganizationName = "Old Name",
            RegistrationNumber = "111",
            City = "Tbilisi",
            PhoneNumber = "+995555000000",
            IsVerified = true,
            VerifiedAt = DateTime.UtcNow.AddDays(-3),
            VerifiedByUserId = 1,
            IsAcceptingApplications = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        _shelterRepository
            .Setup(r => r.GetByUserIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _shelterRepository
            .Setup(r => r.ExistsByRegistrationNumberAsync(
                "405123456",
                12,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await CreateService().UpsertProfileAsync(7, ValidRequest("New Name"));

        Assert.False(existing.IsVerified);
        Assert.Null(existing.VerifiedAt);
        Assert.Null(existing.VerifiedByUserId);
        Assert.Equal("New Name", existing.OrganizationName);
        Assert.False(result.IsVerified);
    }

    [Fact]
    public async Task UpsertProfileAsync_WhenRegistrationNumberIsTaken_Throws()
    {
        _shelterRepository
            .Setup(r => r.GetByUserIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShelterProfile?)null);
        _shelterRepository
            .Setup(r => r.ExistsByRegistrationNumberAsync(
                "405123456",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().UpsertProfileAsync(7, ValidRequest()));
    }

    [Fact]
    public async Task GetPublicProfileAsync_WhenUnverified_Throws()
    {
        _shelterRepository
            .Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShelterProfile
            {
                Id = 5,
                UserId = 7,
                OrganizationName = "Shelter",
                City = "Tbilisi",
                PhoneNumber = "+995555000000",
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            });

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CreateService().GetPublicProfileAsync(5));
    }

    [Fact]
    public async Task VerifyAsync_SetsVerificationFields()
    {
        var profile = new ShelterProfile
        {
            Id = 5,
            UserId = 7,
            OrganizationName = "Shelter",
            City = "Tbilisi",
            PhoneNumber = "+995555000000",
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        _shelterRepository
            .Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await CreateService().VerifyAsync(5, 1);

        Assert.True(profile.IsVerified);
        Assert.NotNull(profile.VerifiedAt);
        Assert.Equal(1, profile.VerifiedByUserId);
        Assert.True(result.IsVerified);
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsProfileAndStats()
    {
        var profile = new ShelterProfile
        {
            Id = 5,
            UserId = 7,
            OrganizationName = "Shelter",
            City = "Tbilisi",
            PhoneNumber = "+995555000000",
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        _shelterRepository
            .Setup(r => r.GetByUserIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _adoptionRepository
            .Setup(r => r.GetShelterStatsAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShelterAdoptionStats
            {
                TotalListings = 4,
                ActiveListings = 2,
                PausedListings = 1,
                AdoptedListings = 1,
                WithdrawnListings = 0,
                TotalApplications = 9,
                PendingApplications = 3,
                ApprovedApplications = 2,
                CompletedAdoptions = 1
            });

        var result = await CreateService().GetDashboardAsync(7);

        Assert.NotNull(result.Shelter);
        Assert.Equal("Shelter", result.Shelter!.OrganizationName);
        Assert.Equal(4, result.Stats.TotalListings);
        Assert.Equal(3, result.Stats.PendingApplications);
        Assert.Equal(1, result.Stats.CompletedAdoptions);
    }

    [Fact]
    public async Task GetAdminListAsync_ReturnsFilteredProfiles()
    {
        var profile = new ShelterProfile
        {
            Id = 5,
            UserId = 7,
            OrganizationName = "Shelter",
            City = "Tbilisi",
            PhoneNumber = "+995555000000",
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        _shelterRepository
            .Setup(r => r.GetPagedAsync(
                It.IsAny<ShelterSearchCriteria>(),
                false,
                0,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ShelterProfile> { profile }, 1));

        var result = await CreateService().GetAdminListAsync(
            new ShelterSearchCriteria(),
            verified: false);

        Assert.Single(result.Items);
        Assert.Equal("Shelter", result.Items[0].OrganizationName);
        Assert.False(result.Items[0].IsVerified);
    }

    [Fact]
    public async Task GrantShelterRoleAsync_WhenPetOwner_ChangesRoleToShelter()
    {
        var user = new User
        {
            Id = 7,
            FirstName = "Nino",
            LastName = "Beridze",
            Role = UserRole.PetOwner
        };

        _userRepository
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await CreateService().GrantShelterRoleAsync(7);

        Assert.Equal(UserRole.Shelter, user.Role);
        Assert.NotNull(user.UpdatedAt);
        _userRepository.Verify(r => r.Update(user), Times.Once);
        _unitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
