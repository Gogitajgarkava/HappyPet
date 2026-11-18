using HappyPet.Application.Dtos.Requests;
using HappyPet.Application.Validators;

namespace HappyPet.Tests.Validators;

public class AdoptionValidatorTests
{
    [Fact]
    public async Task UpdateShelterProfileRequestValidator_WithInvalidValues_ReturnsErrors()
    {
        var validator = new UpdateShelterProfileRequestValidator();
        var request = new UpdateShelterProfileRequest
        {
            OrganizationName = string.Empty,
            City = string.Empty,
            PhoneNumber = "abc",
            Email = "not-an-email",
            Website = "not-a-url",
            Description = new string('x', 2001)
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.OrganizationName));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.City));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.PhoneNumber));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Email));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Website));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Description));
    }

    [Fact]
    public async Task UpdateShelterProfileRequestValidator_WithOptionalFieldsMissing_IsValid()
    {
        var validator = new UpdateShelterProfileRequestValidator();
        var request = new UpdateShelterProfileRequest
        {
            OrganizationName = "Happy Shelter",
            City = "Tbilisi",
            PhoneNumber = "+995555000000"
        };

        var result = await validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CreateAdoptionListingRequestValidator_WithInvalidValues_ReturnsErrors()
    {
        var validator = new CreateAdoptionListingRequestValidator();
        var request = new CreateAdoptionListingRequest
        {
            PetId = 0,
            City = string.Empty,
            AdoptionFee = -1,
            Description = new string('x', 2001)
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.PetId));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.City));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.AdoptionFee));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Description));
    }

    [Fact]
    public async Task CreateAdoptionApplicationRequestValidator_WithInvalidValues_ReturnsErrors()
    {
        var validator = new CreateAdoptionApplicationRequestValidator();
        var request = new CreateAdoptionApplicationRequest
        {
            FullName = string.Empty,
            Email = "not-an-email",
            Phone = "abc",
            Message = new string('x', 2001)
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.FullName));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Email));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Phone));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Message));
    }

    [Fact]
    public async Task ReviewAdoptionApplicationRequestValidator_WithLongNote_ReturnsError()
    {
        var validator = new ReviewAdoptionApplicationRequestValidator();
        var request = new ReviewAdoptionApplicationRequest
        {
            Note = new string('x', 501)
        };

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.Note));
    }
}
