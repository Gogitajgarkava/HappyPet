using HappyPet.Application.Interfaces.Mail;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Options;
using HappyPet.Application.Services;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;
using Microsoft.Extensions.Options;
using Moq;

namespace HappyPet.Tests.Services;

public class ReminderServiceTests
{
    private readonly Mock<IVaccinationRepository> _vaccinationRepository = new();
    private readonly Mock<IReminderRepository> _reminderRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IEmailSender> _emailSender = new();

    public ReminderServiceTests()
    {
        _reminderRepository
            .Setup(r => r.GetAsync(
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<ReminderChannel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((VaccinationReminder?)null);

        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _emailSender
            .Setup(e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Theory]
    [InlineData(30, 30)]
    [InlineData(20, 30)]
    [InlineData(8, 30)]
    [InlineData(7, 7)]
    [InlineData(2, 7)]
    [InlineData(1, 1)]
    [InlineData(0, 0)]
    [InlineData(-5, 0)]
    public async Task RunAsync_SelectsClosestApplicableStage(int daysUntilDue, int expectedOffset)
    {
        var today = DateTime.UtcNow.Date;
        var dueAt = today.AddDays(daysUntilDue);
        var vaccination = BuildVaccination(dueAt);
        VaccinationReminder? added = null;

        _vaccinationRepository
            .Setup(r => r.GetReminderCandidatesAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vaccination> { vaccination });
        _reminderRepository
            .Setup(r => r.AddAsync(It.IsAny<VaccinationReminder>(), It.IsAny<CancellationToken>()))
            .Callback<VaccinationReminder, CancellationToken>((reminder, _) => added = reminder)
            .Returns(Task.CompletedTask);

        var result = await CreateService().RunAsync();

        Assert.Equal(1, result.Sent);
        Assert.NotNull(added);
        Assert.Equal(expectedOffset, added!.OffsetDays);
        Assert.Equal(dueAt.AddDays(-expectedOffset), added.ScheduledFor);
        Assert.Equal(ReminderStatus.Sent, added.Status);
        Assert.Equal(1, added.AttemptCount);
        Assert.NotNull(added.SentAt);
        _emailSender.Verify(e => e.SendEmailAsync(
            "nino@example.com",
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenStageAlreadySent_DoesNotSendAgain()
    {
        var vaccination = BuildVaccination(DateTime.UtcNow.Date.AddDays(30));
        var existing = BuildReminder(vaccination, ReminderStatus.Sent);

        _vaccinationRepository
            .Setup(r => r.GetReminderCandidatesAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vaccination> { vaccination });
        _reminderRepository
            .Setup(r => r.GetAsync(
                vaccination.Id,
                It.IsAny<DateTime>(),
                30,
                ReminderChannel.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await CreateService().RunAsync();

        Assert.Equal(0, result.Sent);
        Assert.Equal(1, result.Skipped);
        _reminderRepository.Verify(
            r => r.AddAsync(It.IsAny<VaccinationReminder>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _emailSender.Verify(
            e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RunAsync_WhenEmailFails_RetriesOnNextRun()
    {
        var vaccination = BuildVaccination(DateTime.UtcNow.Date.AddDays(30));
        VaccinationReminder? added = null;

        _vaccinationRepository
            .Setup(r => r.GetReminderCandidatesAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vaccination> { vaccination });
        _reminderRepository
            .Setup(r => r.AddAsync(It.IsAny<VaccinationReminder>(), It.IsAny<CancellationToken>()))
            .Callback<VaccinationReminder, CancellationToken>((reminder, _) => added = reminder)
            .Returns(Task.CompletedTask);
        _emailSender
            .Setup(e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP მიუწვდომელია"));

        var service = CreateService();
        var firstRun = await service.RunAsync();

        Assert.Equal(1, firstRun.Failed);
        Assert.NotNull(added);
        Assert.Equal(ReminderStatus.Failed, added!.Status);
        Assert.Equal(1, added.AttemptCount);
        Assert.Equal("SMTP მიუწვდომელია", added.LastError);

        _reminderRepository
            .Setup(r => r.GetAsync(
                vaccination.Id,
                It.IsAny<DateTime>(),
                30,
                ReminderChannel.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(added);
        _emailSender
            .Setup(e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var secondRun = await service.RunAsync();

        Assert.Equal(1, secondRun.Sent);
        Assert.Equal(1, secondRun.Retried);
        Assert.Equal(ReminderStatus.Sent, added.Status);
        Assert.Equal(2, added.AttemptCount);
        Assert.Null(added.LastError);
    }

    [Fact]
    public async Task RunAsync_WhenMaxAttemptsReached_DoesNotSendAgain()
    {
        var vaccination = BuildVaccination(DateTime.UtcNow.Date.AddDays(30));
        var existing = BuildReminder(vaccination, ReminderStatus.Failed, attemptCount: 3);

        _vaccinationRepository
            .Setup(r => r.GetReminderCandidatesAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vaccination> { vaccination });
        _reminderRepository
            .Setup(r => r.GetAsync(
                vaccination.Id,
                It.IsAny<DateTime>(),
                30,
                ReminderChannel.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await CreateService().RunAsync();

        Assert.Equal(0, result.Sent);
        Assert.Equal(1, result.Skipped);
        _emailSender.Verify(
            e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RunAsync_WhenOwnerEmailIsNotVerified_Skips()
    {
        var vaccination = BuildVaccination(
            DateTime.UtcNow.Date.AddDays(30),
            ownerVerified: false);

        _vaccinationRepository
            .Setup(r => r.GetReminderCandidatesAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vaccination> { vaccination });

        var result = await CreateService().RunAsync();

        Assert.Equal(0, result.Sent);
        Assert.Equal(1, result.Skipped);
        _reminderRepository.Verify(
            r => r.AddAsync(It.IsAny<VaccinationReminder>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _emailSender.Verify(
            e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RunAsync_WhenPetIsDeceased_Skips()
    {
        var vaccination = BuildVaccination(
            DateTime.UtcNow.Date.AddDays(30),
            petDeceased: true);

        _vaccinationRepository
            .Setup(r => r.GetReminderCandidatesAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vaccination> { vaccination });

        var result = await CreateService().RunAsync();

        Assert.Equal(0, result.Sent);
        Assert.Equal(1, result.Skipped);
        _emailSender.Verify(
            e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMyAsync_MapsReminderHistory()
    {
        var vaccination = BuildVaccination(DateTime.UtcNow.Date.AddDays(30));
        var reminder = BuildReminder(vaccination, ReminderStatus.Sent);

        _reminderRepository
            .Setup(r => r.GetByOwnerPagedAsync(1, 0, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<VaccinationReminder> { reminder }, 1));

        var result = await CreateService().GetMyAsync(1);

        Assert.Equal(1, result.TotalCount);
        var item = Assert.Single(result.Items);
        Assert.Equal(reminder.Id, item.Id);
        Assert.Equal("მურკა", item.PetName);
        Assert.Equal("Rabies", item.VaccineName);
        Assert.Equal(30, item.OffsetDays);
        Assert.Equal(ReminderStatus.Sent, item.Status);
    }

    private ReminderService CreateService() => new(
        _vaccinationRepository.Object,
        _reminderRepository.Object,
        _unitOfWork.Object,
        _emailSender.Object,
        Options.Create(new ReminderOptions
        {
            MaxAttempts = 3,
            OverdueLookbackDays = 30,
            Offsets = new[] { 30, 7, 1, 0 }
        }));

    private static Vaccination BuildVaccination(
        DateTime dueAt,
        bool ownerVerified = true,
        bool petDeceased = false)
    {
        var owner = new User
        {
            Id = 1,
            FirstName = "ნინო",
            LastName = "ჯღარკავა",
            Email = "nino@example.com",
            IsEmailVerified = ownerVerified
        };

        var pet = new Pet
        {
            Id = 10,
            OwnerId = owner.Id,
            Owner = owner,
            Name = "მურკა",
            Species = PetSpecies.Cat,
            IsDeceased = petDeceased
        };

        return new Vaccination
        {
            Id = 20,
            PetId = pet.Id,
            Pet = pet,
            VaccineName = "Rabies",
            VaccinatedAt = dueAt.AddYears(-1),
            NextDueAt = dueAt
        };
    }

    private static VaccinationReminder BuildReminder(
        Vaccination vaccination,
        ReminderStatus status,
        int attemptCount = 1)
    {
        return new VaccinationReminder
        {
            Id = 30,
            VaccinationId = vaccination.Id,
            PetId = vaccination.PetId,
            OwnerUserId = vaccination.Pet.OwnerId,
            PetName = vaccination.Pet.Name,
            VaccineName = vaccination.VaccineName,
            RecipientEmail = vaccination.Pet.Owner.Email,
            DueAt = vaccination.NextDueAt!.Value.Date,
            OffsetDays = 30,
            Channel = ReminderChannel.Email,
            Status = status,
            ScheduledFor = vaccination.NextDueAt!.Value.Date.AddDays(-30),
            AttemptCount = attemptCount,
            CreatedAt = DateTime.UtcNow
        };
    }
}
