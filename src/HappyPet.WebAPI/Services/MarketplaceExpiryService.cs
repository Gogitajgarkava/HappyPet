using HappyPet.Application.Interfaces.Repositories;

namespace HappyPet.WebAPI.Services
{
    public class MarketplaceExpiryService : BackgroundService
    {
        private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan ExpiryInterval = TimeSpan.FromHours(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MarketplaceExpiryService> _logger;

        public MarketplaceExpiryService(
            IServiceScopeFactory scopeFactory,
            ILogger<MarketplaceExpiryService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(InitialDelay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            using var timer = new PeriodicTimer(ExpiryInterval);

            do
            {
                await ExpireAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task ExpireAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IMarketplaceRepository>();
                var expired = await repository.ExpireActiveAsync(DateTime.UtcNow, cancellationToken);

                if (expired > 0)
                {
                    _logger.LogInformation("Expired {Count} marketplace listings.", expired);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to expire marketplace listings.");
            }
        }
    }
}
