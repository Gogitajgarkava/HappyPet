using System.Net;
using System.Text;
using System.Text.Json;
using HappyPet.Tests.Integration;

namespace HappyPet.Tests.Integration;

public class ValidationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ValidationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithEmptyBody_ReturnsValidationEnvelope()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync(
            "/api/v1/auth/login",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = JsonDocument
            .Parse(await response.Content.ReadAsStringAsync())
            .RootElement;

        Assert.Equal("VALIDATION_ERROR", body.GetProperty("code").GetString());
        Assert.True(body.GetProperty("errors").TryGetProperty("Email", out var emailErrors));
        Assert.NotEmpty(emailErrors.EnumerateArray());
        Assert.True(body.GetProperty("errors").TryGetProperty("Password", out _));
    }

    [Fact]
    public async Task VerifyEmail_WithInvalidCode_ReturnsValidationEnvelope()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync(
            "/api/v1/auth/verify-email",
            new StringContent(
                """{"email":"owner@example.com","code":"abc"}""",
                Encoding.UTF8,
                "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = JsonDocument
            .Parse(await response.Content.ReadAsStringAsync())
            .RootElement;

        Assert.Equal("VALIDATION_ERROR", body.GetProperty("code").GetString());
        Assert.True(body.GetProperty("errors").TryGetProperty("Code", out _));
    }

    [Fact]
    public async Task CreateUser_WithInvalidPersonalId_ReturnsValidationEnvelope()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync(
            "/api/v1/user/add-user",
            new StringContent(
                """
                {
                  "personalId": "123",
                  "firstName": "გიორგი",
                  "lastName": "ჯღარკავა",
                  "birthDate": "1990-01-01T00:00:00Z",
                  "email": "not-an-email",
                  "password": "123",
                  "phoneNumber": "+995510000000"
                }
                """,
                Encoding.UTF8,
                "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = JsonDocument
            .Parse(await response.Content.ReadAsStringAsync())
            .RootElement;

        Assert.Equal("VALIDATION_ERROR", body.GetProperty("code").GetString());

        var errors = body.GetProperty("errors");
        Assert.True(errors.TryGetProperty("PersonalId", out _));
        Assert.True(errors.TryGetProperty("Email", out _));
        Assert.True(errors.TryGetProperty("Password", out _));
    }
}
