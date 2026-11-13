using HappyPet.Domain.Enums;

namespace HappyPet.Application.Dtos.Responses
{
    public class VaccinationResponse
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string VaccineName { get; set; } = string.Empty;
        public string? Disease { get; set; }
        public string? Manufacturer { get; set; }
        public string? ManufacturerCountry { get; set; }
        public int? ImmunityYears { get; set; }
        public VaccinationType? VaccinationType { get; set; }
        public bool IsNextDueOverridden { get; set; }
        public string? BatchNumber { get; set; }
        public string? CertificateNumber { get; set; }
        public DateTime VaccinatedAt { get; set; }
        public DateTime? NextDueAt { get; set; }
        public int? DaysUntilDue { get; set; }
        public bool IsOverdue { get; set; }
        public string? VeterinarianName { get; set; }
        public string? ClinicName { get; set; }
        public string? Notes { get; set; }
        public string? AirsVaccinationId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class VaccinationImportResult
    {
        public int Imported { get; set; }
        public int Skipped { get; set; }
        public List<VaccinationResponse> Items { get; set; } = new();
    }
}
