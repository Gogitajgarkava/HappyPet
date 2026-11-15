using HappyPet.Application.Interfaces.Services;
using HappyPet.Application.Options;
using Microsoft.Extensions.Options;

namespace HappyPet.WebAPI.Services
{
    public class VaccinationReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ReminderOptions _options;
        private readonly ILogger<VaccinationReminderBackgroundService> _logger;

        public VaccinationReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            IOptions<ReminderOptions> options,
            ILogger<VaccinationReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Vaccination reminders are disabled.");
                return;
            }

            var timezone = ResolveTimezone(_options.Timezone);
            var runHour = Math.Clamp(_options.RunHourLocal, 0, 23);

            try
            {
                var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezone);
                var nextRunLocal = nowLocal.Date.AddHours(runHour);

                // On startup after the configured hour, run once to catch up.
                // The unique reminder key prevents duplicate deliveries.
                if (nowLocal >= nextRunLocal)
                {
                    await RunAsync(stoppingToken);
                    nextRunLocal = nextRunLocal.AddDays(1);
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    var currentLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezone);
                    var delay = nextRunLocal - currentLocal;
                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, stoppingToken);
                    }

                    await RunAsync(stoppingToken);

                    var nextDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezone).Date.AddDays(1);
                    nextRunLocal = nextDate.AddHours(runHour);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown.
            }
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();

            try
            {
                var result = await reminderService.RunAsync(cancellationToken);
                _logger.LogInformation(
                    "Vaccination reminders finished. Candidates: {Candidates}, sent: {Sent}, failed: {Failed}, skipped: {Skipped}.",
                    result.Candidates,
                    result.Sent,
                    result.Failed,
                    result.Skipped);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vaccination reminder run failed.");
            }
        }

        private TimeZoneInfo ResolveTimezone(string? timezoneId)
        {
            if (!string.IsNullOrWhiteSpace(timezoneId))
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Georgian Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }

            _logger.LogWarning(
                "Reminder timezone '{Timezone}' was not found. Falling back to UTC.",
                timezoneId);
            return TimeZoneInfo.Utc;
        }
    }
}
