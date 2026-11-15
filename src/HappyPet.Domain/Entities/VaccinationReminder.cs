using HappyPet.Domain.Common;
using HappyPet.Domain.Enums;

namespace HappyPet.Domain.Entities
{
    /// <summary>
    /// A single reminder delivery attempt for a vaccination due date.
    /// The unique key is vaccination + due date + offset + channel.
    /// </summary>
    public class VaccinationReminder : BaseEntity
    {
        public int VaccinationId { get; set; }
        public int PetId { get; set; }
        public int OwnerUserId { get; set; }

        public string PetName { get; set; } = string.Empty;
        public string VaccineName { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;

        /// <summary>Normalised vaccination due date used when this reminder was created.</summary>
        public DateTime DueAt { get; set; }

        /// <summary>30, 7, 1 or 0 days before the due date.</summary>
        public int OffsetDays { get; set; }

        public ReminderChannel Channel { get; set; } = ReminderChannel.Email;
        public ReminderStatus Status { get; set; } = ReminderStatus.Pending;

        public DateTime ScheduledFor { get; set; }
        public DateTime? SentAt { get; set; }
        public int AttemptCount { get; set; }
        public string? LastError { get; set; }
        public string? SkipReason { get; set; }

        public Vaccination Vaccination { get; set; } = null!;
    }
}
