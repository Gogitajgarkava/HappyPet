using System.Globalization;
using HappyPet.Application.Dtos.Responses;
using HappyPet.Application.Interfaces.Mail;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Application.Interfaces.Services;
using HappyPet.Application.Options;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;
using Microsoft.Extensions.Options;

namespace HappyPet.Application.Services
{
    public class ReminderService : IReminderService
    {
        private const int DefaultPageSize = 20;
        private const int MaxPageSize = 100;

        private static readonly SemaphoreSlim RunLock = new(1, 1);

        private readonly IVaccinationRepository _vaccinationRepository;
        private readonly IReminderRepository _reminderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly ReminderOptions _options;

        public ReminderService(
            IVaccinationRepository vaccinationRepository,
            IReminderRepository reminderRepository,
            IUnitOfWork unitOfWork,
            IEmailSender emailSender,
            IOptions<ReminderOptions> options)
        {
            _vaccinationRepository = vaccinationRepository;
            _reminderRepository = reminderRepository;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _options = options.Value;
        }

        public async Task<PagedResult<VaccinationReminderResponse>> GetMyAsync(
            int ownerUserId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var (validPage, validPageSize) = NormalizePagination(page, pageSize);
            var (items, totalCount) = await _reminderRepository.GetByOwnerPagedAsync(
                ownerUserId,
                (validPage - 1) * validPageSize,
                validPageSize,
                cancellationToken);

            return ToPagedResult(items, validPage, validPageSize, totalCount);
        }

        public async Task<PagedResult<VaccinationReminderResponse>> GetByPetAsync(
            int petId,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var (validPage, validPageSize) = NormalizePagination(page, pageSize);
            var (items, totalCount) = await _reminderRepository.GetByPetPagedAsync(
                petId,
                (validPage - 1) * validPageSize,
                validPageSize,
                cancellationToken);

            return ToPagedResult(items, validPage, validPageSize, totalCount);
        }

        public async Task<ReminderRunResponse> RunAsync(CancellationToken cancellationToken = default)
        {
            await RunLock.WaitAsync(cancellationToken);

            try
            {
                var now = DateTime.UtcNow;
                var today = now.Date;
                var offsets = NormalizeOffsets(_options.Offsets);
                var maxOffset = offsets.Max();
                var overdueLookbackDays = Math.Max(0, _options.OverdueLookbackDays);
                var maxAttempts = Math.Max(1, _options.MaxAttempts);

                var candidates = await _vaccinationRepository.GetReminderCandidatesAsync(
                    today.AddDays(-overdueLookbackDays),
                    today.AddDays(maxOffset + 1),
                    cancellationToken);

                var result = new ReminderRunResponse
                {
                    RunAt = now,
                    Candidates = candidates.Count
                };

                foreach (var vaccination in candidates)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (vaccination.Pet == null
                        || vaccination.Pet.IsDeceased
                        || vaccination.NextDueAt == null)
                    {
                        result.Skipped++;
                        continue;
                    }

                    var owner = vaccination.Pet.Owner;
                    if (owner == null
                        || !owner.IsEmailVerified
                        || string.IsNullOrWhiteSpace(owner.Email))
                    {
                        result.Skipped++;
                        continue;
                    }

                    var dueAt = NormalizeDate(vaccination.NextDueAt.Value);
                    var daysUntilDue = (dueAt - today).Days;
                    var offset = SelectOffset(daysUntilDue, offsets);

                    if (offset == null)
                    {
                        result.Skipped++;
                        continue;
                    }

                    var reminder = await _reminderRepository.GetAsync(
                        vaccination.Id,
                        dueAt,
                        offset.Value,
                        ReminderChannel.Email,
                        cancellationToken);

                    var isRetry = reminder != null;
                    if (reminder != null)
                    {
                        if (reminder.Status == ReminderStatus.Sent
                            || reminder.Status == ReminderStatus.Skipped
                            || reminder.AttemptCount >= maxAttempts)
                        {
                            result.Skipped++;
                            continue;
                        }
                    }
                    else
                    {
                        reminder = new VaccinationReminder
                        {
                            VaccinationId = vaccination.Id,
                            PetId = vaccination.PetId,
                            OwnerUserId = owner.Id,
                            PetName = vaccination.Pet.Name,
                            VaccineName = vaccination.VaccineName,
                            RecipientEmail = owner.Email.Trim(),
                            DueAt = dueAt,
                            OffsetDays = offset.Value,
                            Channel = ReminderChannel.Email,
                            Status = ReminderStatus.Pending,
                            ScheduledFor = dueAt.AddDays(-offset.Value),
                            CreatedAt = now
                        };

                        await _reminderRepository.AddAsync(reminder, cancellationToken);
                    }

                    // The owner may have changed since an earlier failed attempt.
                    reminder.OwnerUserId = owner.Id;
                    reminder.PetName = vaccination.Pet.Name;
                    reminder.VaccineName = vaccination.VaccineName;
                    reminder.RecipientEmail = owner.Email.Trim();
                    reminder.Status = ReminderStatus.Pending;
                    reminder.AttemptCount++;
                    reminder.LastError = null;
                    reminder.SkipReason = null;
                    reminder.UpdatedAt = now;

                    // Reserve the unique reminder row before delivery.
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    try
                    {
                        await _emailSender.SendEmailAsync(
                            reminder.RecipientEmail,
                            BuildSubject(vaccination),
                            BuildBody(vaccination, dueAt, daysUntilDue),
                            cancellationToken);

                        reminder.Status = ReminderStatus.Sent;
                        reminder.SentAt = DateTime.UtcNow;
                        reminder.UpdatedAt = DateTime.UtcNow;
                        result.Sent++;

                        if (isRetry)
                        {
                            result.Retried++;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        reminder.Status = ReminderStatus.Failed;
                        reminder.LastError = Truncate(ex.Message, 500);
                        reminder.UpdatedAt = DateTime.UtcNow;
                        result.Failed++;
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                return result;
            }
            finally
            {
                RunLock.Release();
            }
        }

        private static int? SelectOffset(int daysUntilDue, int[] offsets)
        {
            if (daysUntilDue <= 0)
            {
                return offsets[0];
            }

            foreach (var offset in offsets)
            {
                if (offset >= daysUntilDue)
                {
                    return offset;
                }
            }

            return null;
        }

        private static int[] NormalizeOffsets(int[]? configured)
        {
            var offsets = (configured ?? Array.Empty<int>())
                .Where(offset => offset is >= 0 and <= 365)
                .Distinct()
                .OrderBy(offset => offset)
                .ToList();

            if (!offsets.Contains(0))
            {
                offsets.Insert(0, 0);
            }

            return offsets.ToArray();
        }

        private static DateTime NormalizeDate(DateTime value)
        {
            if (value.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
            }

            return DateTime.SpecifyKind(value.ToUniversalTime().Date, DateTimeKind.Utc);
        }

        private static string BuildSubject(Vaccination vaccination)
        {
            return $"HappyPet — ვაქცინაციის შეხსენება: {vaccination.Pet.Name}";
        }

        private static string BuildBody(Vaccination vaccination, DateTime dueAt, int daysUntilDue)
        {
            var ownerName = vaccination.Pet.Owner?.FirstName;
            var greeting = string.IsNullOrWhiteSpace(ownerName)
                ? "გამარჯობა!"
                : $"გამარჯობა, {ownerName}!";

            var dueText = dueAt.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            var statusText = daysUntilDue > 0
                ? $"ვაქცინაციამდე დარჩენილია {daysUntilDue} დღე."
                : daysUntilDue == 0
                    ? "ვაქცინაციის ვადა დღეს იწურება."
                    : $"ვაქცინაციის ვადა გადაცილებულია {Math.Abs(daysUntilDue)} დღით.";

            return $"{greeting}\n\n"
                   + $"შეგახსენებთ, რომ {vaccination.Pet.Name}-ს ვაქცინაციის ვადა ეწურება {dueText}.\n"
                   + $"ვაქცინა: {vaccination.VaccineName}\n"
                   + $"{statusText}\n\n"
                   + "დეტალები იხილეთ HappyPet-ის აპლიკაციაში.\n\n"
                   + "პატივისცემით,\nHappyPet";
        }

        private static string Truncate(string value, int maxLength)
        {
            return value.Length <= maxLength ? value : value[..maxLength];
        }

        private static (int Page, int PageSize) NormalizePagination(int page, int pageSize)
        {
            var validPage = page < 1 ? 1 : page;
            var validPageSize = pageSize switch
            {
                < 1 => DefaultPageSize,
                > MaxPageSize => MaxPageSize,
                _ => pageSize
            };

            return (validPage, validPageSize);
        }

        private static PagedResult<VaccinationReminderResponse> ToPagedResult(
            List<VaccinationReminder> items,
            int page,
            int pageSize,
            int totalCount)
        {
            return new PagedResult<VaccinationReminderResponse>
            {
                Items = items.Select(MapReminder).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        private static VaccinationReminderResponse MapReminder(VaccinationReminder reminder)
        {
            return new VaccinationReminderResponse
            {
                Id = reminder.Id,
                VaccinationId = reminder.VaccinationId,
                PetId = reminder.PetId,
                PetName = reminder.PetName,
                VaccineName = reminder.VaccineName,
                DueAt = reminder.DueAt,
                OffsetDays = reminder.OffsetDays,
                Channel = reminder.Channel,
                Status = reminder.Status,
                ScheduledFor = reminder.ScheduledFor,
                SentAt = reminder.SentAt,
                AttemptCount = reminder.AttemptCount,
                LastError = reminder.LastError,
                SkipReason = reminder.SkipReason,
                CreatedAt = reminder.CreatedAt
            };
        }
    }
}
