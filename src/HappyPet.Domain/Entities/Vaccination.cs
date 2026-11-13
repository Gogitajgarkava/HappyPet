using HappyPet.Domain.Common;
using HappyPet.Domain.Enums;

namespace HappyPet.Domain.Entities
{
    public class Vaccination : BaseEntity
    {
        public int PetId { get; set; }
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
        public string? VeterinarianName { get; set; }
        public string? ClinicName { get; set; }
        public string? Notes { get; set; }

        /// <summary>AIRS vaccination identifier when the record was imported from AIRS.</summary>
        public string? AirsVaccinationId { get; set; }

        public Pet Pet { get; set; } = null!;
    }
}
