using System.Net;
using System.Text;
using System.Text.Json;
using HappyPet.Tests.Integration;
using Xunit;

namespace HappyPet.Tests.Integration;

public class RateLimitingTests : IClassFixture<TestWebApplicationFactory>
{
    private const int PermitLimit = 10;

    private readonly TestWebApplicationFactory _factory;

    public RateLimitingTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_AfterPermitLimit_Returns429WithUnifiedError()
    {
        var client = _factory.CreateClient();
        HttpResponseMessage? lastResponse = null;

        for (var attempt = 0; attempt < PermitLimit + 1; attempt++)
        {
            lastResponse = await client.PostAsync(
                "/api/v1/auth/login",
                new StringContent("{}", Encoding.UTF8, "application/json"));
        }

        Assert.NotNull(lastResponse);
        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse!.StatusCode);

        var body = JsonDocument
            .Parse(await lastResponse.Content.ReadAsStringAsync())
            .RootElement;

        Assert.Equal("RATE_LIMIT_EXCEEDED", body.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("message").GetString()));
        Assert.True(lastResponse.Headers.Contains("Retry-After"));
    }
}
