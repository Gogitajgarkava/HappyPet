using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using HappyPet.Application.Interfaces.Storage;
using Microsoft.Extensions.Configuration;

namespace HappyPet.Infrastructure.Storage
{
    public class S3FileStorageService : IFileStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly string _bucketName;
        private readonly string _region;
        private readonly string? _publicBaseUrl;
        private readonly Lazy<AmazonS3Client> _client;

        public S3FileStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
            _bucketName = configuration["Aws:S3:BucketName"] ?? string.Empty;
            _region = configuration["Aws:S3:Region"] ?? "eu-central-1";
            _publicBaseUrl = configuration["Aws:S3:PublicBaseUrl"];
            _client = new Lazy<AmazonS3Client>(CreateClient);
        }

        public async Task<StoredFile> UploadAsync(
            Stream stream,
            string folder,
            string extension,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            EnsureConfigured();

            var key = $"{folder.Trim('/')}/{Guid.NewGuid():N}{extension}";

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = contentType,
                AutoCloseStream = false
            };

            await Client.PutObjectAsync(request, cancellationToken);

            return new StoredFile
            {
                Key = key,
                Url = GetUrl(key)
            };
        }

        public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            EnsureConfigured();

            await Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            }, cancellationToken);
        }

        public string GetUrl(string key)
        {
            if (!string.IsNullOrWhiteSpace(_publicBaseUrl))
            {
                return $"{_publicBaseUrl.TrimEnd('/')}/{key}";
            }

            return $"https://{_bucketName}.s3.{_region}.amazonaws.com/{key}";
        }

        private AmazonS3Client Client => _client.Value;

        private AmazonS3Client CreateClient()
        {
            var s3Config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_region)
            };

            var serviceUrl = _configuration["Aws:S3:ServiceUrl"];
            if (!string.IsNullOrWhiteSpace(serviceUrl))
            {
                s3Config.ServiceURL = serviceUrl;
                s3Config.ForcePathStyle = true;
                s3Config.UseHttp = serviceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
            }

            var accessKeyId = _configuration["Aws:S3:AccessKeyId"];
            var secretAccessKey = _configuration["Aws:S3:SecretAccessKey"];

            if (!string.IsNullOrWhiteSpace(accessKeyId) && !string.IsNullOrWhiteSpace(secretAccessKey))
            {
                return new AmazonS3Client(new BasicAWSCredentials(accessKeyId, secretAccessKey), s3Config);
            }

            return new AmazonS3Client(s3Config);
        }

        private void EnsureConfigured()
        {
            if (string.IsNullOrWhiteSpace(_bucketName))
            {
                throw new InvalidOperationException("S3 bucket არ არის კონფიგურირებული (Aws:S3:BucketName).");
            }
        }
    }
}
