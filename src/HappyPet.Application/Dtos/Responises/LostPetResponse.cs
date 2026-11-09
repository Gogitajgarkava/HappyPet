using HappyPet.Domain.Enums;

namespace HappyPet.Application.Dtos.Responses
{
    /// <summary>
    /// Public projection of a lost pet. Contains no owner identifiers or personal data.
    /// </summary>
    public class LostPetResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public PetSpecies Species { get; set; }
        public string? Breed { get; set; }
        public PetGender Gender { get; set; }
        public string? Color { get; set; }
        public string? DistinguishingMarks { get; set; }

        public DateTime? LostAt { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public double? LastSeenLatitude { get; set; }
        public double? LastSeenLongitude { get; set; }
        public string? LastSeenAddress { get; set; }
        public string? ContactPhone { get; set; }

        /// <summary>Distance from the search origin in kilometres, when a geo filter was applied.</summary>
        public double? DistanceKm { get; set; }

        public DateTime CreatedAt { get; set; }
        public List<PetPhotoResponse> Photos { get; set; } = new();
    }
}
