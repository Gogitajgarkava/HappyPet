using System.Collections.Generic;

namespace HappyPet.Application.Dtos.Airs
{
    public class AirsAnimalDetails
    {
        public AirsAnimal Animal { get; set; } = new();
        public List<AirsVaccination> Vaccinations { get; set; } = new();
    }

    public class AirsAnimal
    {
        public string Id { get; set; } = string.Empty;
        public string MicrochipNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string? BirthDate { get; set; }
        public string Color { get; set; } = string.Empty;
        public string DistinguishingMarks { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public AirsOwner? Owner { get; set; }
    }

    public class AirsOwner
    {
        public string PersonalId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class AirsVaccination
    {
        public string Id { get; set; } = string.Empty;
        public string AnimalId { get; set; } = string.Empty;
        public string MicrochipNumber { get; set; } = string.Empty;
        public string VaccineName { get; set; } = string.Empty;
        public string Disease { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public string AdministeredAt { get; set; } = string.Empty;
        public string? NextDueAt { get; set; }
        public string VeterinarianName { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string CertificateNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
