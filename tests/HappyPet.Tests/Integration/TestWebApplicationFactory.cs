using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace HappyPet.Tests.Integration;

/// <summary>
/// Starts the real API pipeline without a database or AIRS connection.
/// Used to test middleware-level behavior such as validation and rate limiting.
/// </summary>
public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
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
                ["RateLimiting:LoginPermitLimit"] = "10",
                ["RateLimiting:LoginWindowMinutes"] = "5",
                ["RateLimiting:OtpPermitLimit"] = "10",
                ["RateLimiting:OtpWindowMinutes"] = "10"
            });
        });
    }
}
