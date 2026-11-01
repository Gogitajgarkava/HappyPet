using HappyPet.Application.Interfaces.Repositories;

namespace HappyPet.WebAPI.Services
{
    public class RefreshTokenCleanupService : BackgroundService
    {
        private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(24);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RefreshTokenCleanupService> _logger;

        public RefreshTokenCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<RefreshTokenCleanupService> logger)
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

            using var timer = new PeriodicTimer(CleanupInterval);

            do
            {
                await CleanupAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task CleanupAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
                var deleted = await repository.DeleteExpiredAsync(DateTime.UtcNow, cancellationToken);

                if (deleted > 0)
                {
                    _logger.LogInformation("Deleted {Count} expired refresh tokens.", deleted);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean up expired refresh tokens.");
            }
        }
    }
}
