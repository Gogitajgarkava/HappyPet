using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Validators;
using HappyPet.Domain.Enums;

namespace HappyPet.Tests.Validators;

public class ValidatorTests
{
    [Fact]
    public async Task CreateUserRequestValidator_WithInvalidValues_ReturnsErrors()
    {
        var validator = new CreateUserRequestValidator();
        var request = new CreateUserRequest
        {
            PersonalId = "123",
            FirstName = string.Empty,
            LastName = "ჯღარკავა",
            BirthDate = default,
            Email = "not-an-email",
            Password = "123",
            PhoneNumber = "+995510000000"
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.PersonalId));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.FirstName));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.BirthDate));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Email));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Password));
    }

    [Fact]
    public async Task CreateUserRequestValidator_WithValidValues_Passes()
    {
        var validator = new CreateUserRequestValidator();
        var request = new CreateUserRequest
        {
            PersonalId = "01000000001",
            FirstName = "გიორგი",
            LastName = "ჯღარკავა",
            BirthDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Email = "owner@example.com",
            Password = "secret123",
            PhoneNumber = "+995510000000",
            Address = "ბათუმი"
        };

        var result = await validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ResetPasswordRequestValidator_WithShortPasswordAndInvalidCode_ReturnsErrors()
    {
        var validator = new ResetPasswordRequestValidator();
        var request = new ResetPasswordRequest
        {
            Email = "owner@example.com",
            Code = "123",
            NewPassword = "123"
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Code));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.NewPassword));
    }

    [Fact]
    public async Task CreatePetRequestValidator_WithEmptyNameAndFutureBirthDate_ReturnsErrors()
    {
        var validator = new CreatePetRequestValidator();
        var request = new CreatePetRequest
        {
            Name = " ",
            Species = (PetSpecies)99,
            Gender = PetGender.Female,
            BirthDate = DateTime.UtcNow.AddDays(1)
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Name));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Species));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.BirthDate));
    }

    [Fact]
    public async Task CreatePetRequestValidator_WithEmptyMicrochip_Passes()
    {
        var validator = new CreatePetRequestValidator();
        var request = new CreatePetRequest
        {
            Name = "შერი",
            Species = PetSpecies.Dog,
            Gender = PetGender.Female,
            MicrochipNumber = "   "
        };

        var result = await validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task UpdatePetRequestValidator_WithEmptyMicrochip_ReturnsError()
    {
        var validator = new UpdatePetRequestValidator();
        var request = new UpdatePetRequest
        {
            Name = "შერი",
            Species = PetSpecies.Dog,
            Gender = PetGender.Female,
            MicrochipNumber = string.Empty
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.MicrochipNumber));
    }

    [Fact]
    public async Task ReportLostPetRequestValidator_WithInvalidCoordinates_ReturnsErrors()
    {
        var validator = new ReportLostPetRequestValidator();
        var request = new ReportLostPetRequest
        {
            LastSeenAt = DateTime.UtcNow.AddDays(1),
            Latitude = 95,
            Longitude = 200,
            LastSeenAddress = new string('a', 301),
            ContactPhone = "abc"
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.LastSeenAt));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Latitude));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Longitude));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.LastSeenAddress));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.ContactPhone));
    }

    [Fact]
    public async Task ReportLostPetRequestValidator_WithValidValues_Passes()
    {
        var validator = new ReportLostPetRequestValidator();
        var request = new ReportLostPetRequest
        {
            LastSeenAt = DateTime.UtcNow.AddHours(-1),
            Latitude = 41.7151,
            Longitude = 44.8271,
            LastSeenAddress = "ვაკე, თბილისი",
            ContactPhone = "+995510000000"
        };

        var result = await validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CreateVaccinationRequestValidator_WithInvalidValues_ReturnsErrors()
    {
        var validator = new CreateVaccinationRequestValidator();
        var request = new CreateVaccinationRequest
        {
            VaccineName = " ",
            VaccinatedAt = DateTime.UtcNow.AddDays(1),
            NextDueAt = DateTime.UtcNow.AddDays(-1),
            Notes = new string('a', 501)
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.VaccineName));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.VaccinatedAt));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.NextDueAt));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Notes));
    }

    [Fact]
    public async Task CreateVaccinationRequestValidator_WithValidValues_Passes()
    {
        var validator = new CreateVaccinationRequestValidator();
        var vaccinatedAt = DateTime.UtcNow.AddDays(-10);
        var request = new CreateVaccinationRequest
        {
            VaccineName = "Rabies",
            Disease = "რაბიესი",
            Manufacturer = "MSD",
            BatchNumber = "B-1",
            VaccinatedAt = vaccinatedAt,
            NextDueAt = vaccinatedAt.AddYears(1),
            VeterinarianName = "ექიმი",
            ClinicName = "კლინიკა",
            Notes = "კარგად"
        };

        var result = await validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }
}
