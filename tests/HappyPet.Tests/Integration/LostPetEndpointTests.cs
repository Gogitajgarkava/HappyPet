using System.Net;
using HappyPet.Tests.Integration;

namespace HappyPet.Tests.Integration;

public class LostPetEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public LostPetEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetLostPets_DoesNotRequireAuthentication()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/pet/lost");

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPetById_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/pet/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetVaccinations_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/pet/1/vaccinations");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDueVaccinations_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/pet/vaccinations/due");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
