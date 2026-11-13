using HappyPet.Domain.Enums;

namespace HappyPet.Application.Dtos.Requests
{
    public class CreateVaccinationRequest
    {
        public string VaccineName { get; set; } = string.Empty;
        public string? Disease { get; set; }
        public string? Manufacturer { get; set; }
        public string? ManufacturerCountry { get; set; }
        public int? ImmunityYears { get; set; }
        public VaccinationType? VaccinationType { get; set; }
        public bool OverrideNextDueDate { get; set; }
        public string? BatchNumber { get; set; }
        public string? CertificateNumber { get; set; }
        public DateTime VaccinatedAt { get; set; }
        public DateTime? NextDueAt { get; set; }
        public string? VeterinarianName { get; set; }
        public string? ClinicName { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateVaccinationRequest
    {
        public string VaccineName { get; set; } = string.Empty;
        public string? Disease { get; set; }
        public string? Manufacturer { get; set; }
        public string? ManufacturerCountry { get; set; }
        public int? ImmunityYears { get; set; }
        public VaccinationType? VaccinationType { get; set; }
        public bool OverrideNextDueDate { get; set; }
        public string? BatchNumber { get; set; }
        public string? CertificateNumber { get; set; }
        public DateTime VaccinatedAt { get; set; }
        public DateTime? NextDueAt { get; set; }
        public string? VeterinarianName { get; set; }
        public string? ClinicName { get; set; }
        public string? Notes { get; set; }
    }
}
