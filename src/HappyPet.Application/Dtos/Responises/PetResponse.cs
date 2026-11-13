using HappyPet.Domain.Enums;

namespace HappyPet.Application.Dtos.Responses
{
    public class PetResponse
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public PetSpecies Species { get; set; }
        public string? Breed { get; set; }
        public string MicrochipNumber { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public PetGender Gender { get; set; }
        public string? Color { get; set; }
        public string? DistinguishingMarks { get; set; }
        public PetSize? Size { get; set; }
        public bool IsSterilized { get; set; }
        public DateTime? SterilizedAt { get; set; }
        public bool IsLost { get; set; }
        public DateTime? LostAt { get; set; }
        public DateTime? FoundAt { get; set; }
        public bool IsDeceased { get; set; }
        public DateTime? DeceasedAt { get; set; }
        public string? DeathCause { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public double? LastSeenLatitude { get; set; }
        public double? LastSeenLongitude { get; set; }
        public string? LastSeenAddress { get; set; }
        public string? ContactPhone { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PetPhotoResponse> Photos { get; set; } = new();
    }
}
