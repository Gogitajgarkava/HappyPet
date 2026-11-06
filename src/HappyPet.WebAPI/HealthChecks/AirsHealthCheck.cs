using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HappyPet.WebAPI.HealthChecks
{
    /// <summary>
    /// Checks that the AIRS service is reachable. AIRS is an external dependency,
    /// so an outage is reported as degraded rather than unhealthy.
    /// </summary>
    public sealed class AirsHealthCheck : IHealthCheck
    {
        public const string HttpClientName = "airs-health";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AirsHealthCheck> _logger;

        public AirsHealthCheck(
            IHttpClientFactory httpClientFactory,
            ILogger<AirsHealthCheck> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(HttpClientName);
                using var response = await client.GetAsync("health", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Healthy("AIRS არის ხელმისაწვდომი.");
                }

                return HealthCheckResult.Degraded(
                    $"AIRS დააბრუნა სტატუსი {(int)response.StatusCode}.");
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "AIRS health check failed.");
                return HealthCheckResult.Degraded("AIRS არ არის ხელმისაწვდომი.", ex);
            }
        }
    }
}
