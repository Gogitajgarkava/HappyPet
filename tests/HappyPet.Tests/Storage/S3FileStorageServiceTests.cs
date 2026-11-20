using HappyPet.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;

namespace HappyPet.Tests.Storage;

public class S3FileStorageServiceTests
{
    [Fact]
    public void GetUrl_WhenPresignedModeEnabled_ReturnsSignedS3Url()
    {
        var service = new S3FileStorageService(BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["Aws:S3:PublicBaseUrl"] = "https://cdn.example.com",
                ["Aws:S3:UsePresignedUrls"] = "true",
                ["Aws:S3:PresignedUrlMinutes"] = "60"
            }));

        var url = service.GetUrl("pets/7/photo.jpg");

        Assert.StartsWith("https://happypetproject.s3.eu-north-1.amazonaws.com/pets/7/photo.jpg", url);
        Assert.Contains("X-Amz-Signature=", url);
        Assert.Contains("X-Amz-Expires=3600", url);
        Assert.Contains("X-Amz-Credential=", url);
        Assert.DoesNotContain("cdn.example.com", url);
    }

    [Fact]
    public void GetUrl_WhenPresignedModeDisabled_UsesPublicBaseUrl()
    {
        var service = new S3FileStorageService(BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["Aws:S3:PublicBaseUrl"] = "https://cdn.example.com/",
                ["Aws:S3:UsePresignedUrls"] = "false"
            }));

        var url = service.GetUrl("pets/7/photo.jpg");

        Assert.Equal("https://cdn.example.com/pets/7/photo.jpg", url);
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> overrides)
    {
        var values = new Dictionary<string, string?>
        {
            ["Aws:S3:BucketName"] = "happypetproject",
            ["Aws:S3:Region"] = "eu-north-1",
            ["Aws:S3:AccessKeyId"] = "AKIAIOSFODNN7EXAMPLE",
            ["Aws:S3:SecretAccessKey"] = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY"
        };

        foreach (var pair in overrides)
        {
            values[pair.Key] = pair.Value;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
