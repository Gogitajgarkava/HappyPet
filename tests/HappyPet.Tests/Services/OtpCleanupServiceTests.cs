using HappyPet.Application.Interfaces.Repositories;
using HappyPet.WebAPI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace HappyPet.Tests.Services;

public class OtpCleanupServiceTests
{
    [Fact]
    public async Task CleanupAsync_UsesOneDayRetentionCutoff()
    {
        DateTime? cutoff = null;
        var repository = new Mock<IUserRepository>();
        repository
            .Setup(r => r.DeleteExpiredOtpCodesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Callback<DateTime, CancellationToken>((value, _) => cutoff = value)
            .ReturnsAsync(2);

        var service = CreateService(repository.Object);

        var before = DateTime.UtcNow.AddHours(-24);
        await service.CleanupAsync(CancellationToken.None);
        var after = DateTime.UtcNow.AddHours(-24);

        Assert.NotNull(cutoff);
        Assert.InRange(cutoff!.Value, before, after);
        repository.Verify(
            r => r.DeleteExpiredOtpCodesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CleanupAsync_SwallowsRepositoryFailure()
    {
        var repository = new Mock<IUserRepository>();
        repository
            .Setup(r => r.DeleteExpiredOtpCodesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("database is unavailable"));

        var service = CreateService(repository.Object);

        await service.CleanupAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CleanupAsync_PropagatesCancellation()
    {
        var repository = new Mock<IUserRepository>();
        repository
            .Setup(r => r.DeleteExpiredOtpCodesAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var service = CreateService(repository.Object);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.CleanupAsync(cts.Token));
    }

    private static OtpCleanupService CreateService(IUserRepository repository)
    {
        var provider = new Mock<IServiceProvider>();
        provider
            .Setup(p => p.GetService(typeof(IUserRepository)))
            .Returns(repository);

        var scope = new Mock<IServiceScope>();
        scope.SetupGet(s => s.ServiceProvider).Returns(provider.Object);

        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

        return new OtpCleanupService(
            scopeFactory.Object,
            Mock.Of<ILogger<OtpCleanupService>>());
    }
}
