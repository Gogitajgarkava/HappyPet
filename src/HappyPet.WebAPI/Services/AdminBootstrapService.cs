using System.Security.Cryptography;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Security;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;

namespace HappyPet.WebAPI.Services
{
    /// <summary>
    /// Creates or promotes the configured bootstrap administrator.
    /// Safe to run on every startup: it is idempotent.
    /// </summary>
    public sealed class AdminBootstrapService : IHostedService
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminBootstrapService> _logger;

        public AdminBootstrapService(
            IServiceProvider services,
            IConfiguration configuration,
            ILogger<AdminBootstrapService> logger)
        {
            _services = services;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_configuration.GetValue<bool>("AdminBootstrap:Enabled"))
            {
                return;
            }

            var email = _configuration["AdminBootstrap:Email"]?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Admin bootstrap is enabled but AdminBootstrap:Email is empty.");
                return;
            }

            using var scope = _services.CreateScope();
            var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                var existing = await users.GetByEmailAsync(email, cancellationToken);

                if (existing is not null)
                {
                    var changed = false;
                    if (existing.Role != UserRole.Admin)
                    {
                        existing.Role = UserRole.Admin;
                        changed = true;
                    }

                    if (!existing.IsEmailVerified)
                    {
                        existing.IsEmailVerified = true;
                        existing.EmailVerifiedAt ??= DateTime.UtcNow;
                        changed = true;
                    }

                    if (_configuration.GetValue<bool>("AdminBootstrap:ForcePasswordReset"))
                    {
                        var resetPassword = _configuration["AdminBootstrap:Password"];
                        if (!string.IsNullOrWhiteSpace(resetPassword))
                        {
                            existing.Password = passwordHasher.HashPassword(resetPassword);
                            changed = true;
                            _logger.LogWarning(
                                "Bootstrap administrator {Email} password was reset because AdminBootstrap:ForcePasswordReset is enabled.",
                                email);
                        }
                    }

                    if (changed)
                    {
                        existing.UpdatedAt = DateTime.UtcNow;
                        users.Update(existing);
                        await unitOfWork.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation("Bootstrap user {Email} was promoted to Admin.", email);
                    }

                    return;
                }

                var password = _configuration["AdminBootstrap:Password"];
                if (string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning(
                        "Admin bootstrap user {Email} does not exist and no AdminBootstrap:Password is configured.",
                        email);
                    return;
                }

                var personalId = await ResolvePersonalIdAsync(users, cancellationToken);
                var birthDate = _configuration.GetValue<DateTime?>("AdminBootstrap:BirthDate")
                    ?? new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                var admin = new User
                {
                    PersonalId = personalId,
                    FirstName = _configuration["AdminBootstrap:FirstName"]?.Trim() ?? "HappyPet",
                    LastName = _configuration["AdminBootstrap:LastName"]?.Trim() ?? "Admin",
                    BirthDate = DateTime.SpecifyKind(birthDate.Date, DateTimeKind.Utc),
                    Email = email,
                    Password = passwordHasher.HashPassword(password),
                    PhoneNumber = _configuration["AdminBootstrap:PhoneNumber"]?.Trim() ?? "+995555000000",
                    Address = _configuration["AdminBootstrap:Address"]?.Trim(),
                    Role = UserRole.Admin,
                    IsEmailVerified = true,
                    EmailVerifiedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await users.AddAsync(admin, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Bootstrap administrator {Email} created. Change the bootstrap password after the first login.",
                    email);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Admin bootstrap failed for {Email}.", email);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task<string> ResolvePersonalIdAsync(
            IUserRepository users,
            CancellationToken cancellationToken)
        {
            var configured = _configuration["AdminBootstrap:PersonalId"]?.Trim();
            if (!string.IsNullOrWhiteSpace(configured)
                && !await users.ExistsByPersonalIdAsync(configured, cancellationToken))
            {
                return configured;
            }

            for (var attempt = 0; attempt < 20; attempt++)
            {
                var candidate = RandomNumberGenerator.GetInt32(0, 1_000_000_000).ToString("D11");
                if (!await users.ExistsByPersonalIdAsync(candidate, cancellationToken))
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException("Could not generate a unique personal id for the bootstrap administrator.");
        }
    }
}
