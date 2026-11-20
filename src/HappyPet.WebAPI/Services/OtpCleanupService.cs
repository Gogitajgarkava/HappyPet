using HappyPet.Application.Interfaces.Repositories;

namespace HappyPet.WebAPI.Services
{
    public class OtpCleanupService : BackgroundService
    {
        private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(24);
        private static readonly TimeSpan RetentionPeriod = TimeSpan.FromHours(24);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OtpCleanupService> _logger;

        public OtpCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<OtpCleanupService> logger)
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

        internal async Task CleanupAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var deleted = await repository.DeleteExpiredOtpCodesAsync(
                    DateTime.UtcNow - RetentionPeriod,
                    cancellationToken);

                if (deleted > 0)
                {
                    _logger.LogInformation("Deleted {Count} expired or consumed OTP codes.", deleted);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean up expired or consumed OTP codes.");
            }
        }
    }
}
