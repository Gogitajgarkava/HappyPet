using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace HappyPet.Tests.Integration;

public class RateLimitingTests : IClassFixture<RateLimitingTests.Factory>
{
    private const int PermitLimit = 10;

    private readonly Factory _factory;

    public RateLimitingTests(Factory factory)
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

    public sealed class Factory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Secret"] = new string('s', 32),
                    ["Jwt:Issuer"] = "HappyPetApi",
                    ["Jwt:Audience"] = "HappyPetClient",
                    ["Jwt:RefreshTokenHashKey"] = new string('h', 32),
                    ["Airs:BaseUrl"] = "http://127.0.0.1:1",
                    ["ConnectionStrings:DefaultConnection"] =
                        "Host=127.0.0.1;Port=1;Database=HappyPetTest;Username=test;Password=test",
                    ["RateLimiting:LoginPermitLimit"] = PermitLimit.ToString(),
                    ["RateLimiting:LoginWindowMinutes"] = "5",
                    ["RateLimiting:OtpPermitLimit"] = "10",
                    ["RateLimiting:OtpWindowMinutes"] = "10"
                });
            });
        }
    }
}
