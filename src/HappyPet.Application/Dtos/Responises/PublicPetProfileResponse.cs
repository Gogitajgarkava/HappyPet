using HappyPet.Domain.Enums;

namespace HappyPet.Application.Dtos.Responses
{
    public class PublicProfileResponse
    {
        public int PetId { get; set; }
        public bool IsEnabled { get; set; }
        public string? Token { get; set; }
        public string? PublicProfilePath { get; set; }
        public string? PublicProfileUrl { get; set; }
    }

    public class PublicPetProfileResponse
    {
        public string Name { get; set; } = string.Empty;
        public PetSpecies Species { get; set; }
        public string? Breed { get; set; }
        public PetGender Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Color { get; set; }
        public string? DistinguishingMarks { get; set; }
        public PetSize? Size { get; set; }
        public bool IsSterilized { get; set; }
        public bool IsLost { get; set; }
        public DateTime? LostAt { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public string? LastSeenAddress { get; set; }
        public string? ContactPhone { get; set; }
        public bool IsDeceased { get; set; }
        public DateTime? DeceasedAt { get; set; }
        public List<PublicPetPhotoResponse> Photos { get; set; } = new();
        public PublicVaccinationSummary Vaccinations { get; set; } = new();
    }

    public class PublicPetPhotoResponse
    {
        public string Url { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }

    public class PublicVaccinationSummary
    {
        public bool IsVaccinated { get; set; }
        public int Count { get; set; }
        public DateTime? LastVaccinatedAt { get; set; }
        public DateTime? NextDueAt { get; set; }
        public bool IsOverdue { get; set; }
    }
}
