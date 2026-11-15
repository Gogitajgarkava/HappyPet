using HappyPet.Domain.Enums;

namespace HappyPet.Application.Dtos.Responses
{
    public class VaccinationReminderResponse
    {
        public int Id { get; set; }
        public int VaccinationId { get; set; }
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string VaccineName { get; set; } = string.Empty;
        public DateTime DueAt { get; set; }
        public int OffsetDays { get; set; }
        public ReminderChannel Channel { get; set; }
        public ReminderStatus Status { get; set; }
        public DateTime ScheduledFor { get; set; }
        public DateTime? SentAt { get; set; }
        public int AttemptCount { get; set; }
        public string? LastError { get; set; }
        public string? SkipReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ReminderRunResponse
    {
        public DateTime RunAt { get; set; }
        public int Candidates { get; set; }
        public int Sent { get; set; }
        public int Failed { get; set; }
        public int Skipped { get; set; }
        public int Retried { get; set; }
    }
}
