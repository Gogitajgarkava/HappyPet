using HappyPet.Domain.Enums;

namespace HappyPet.Application.Dtos.Responses
{
    public class PetResponse
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public PetSpecies Species { get; set; }
        public string? Breed { get; set; }
        public string MicrochipNumber { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public PetGender Gender { get; set; }
        public string? Color { get; set; }
        public string? DistinguishingMarks { get; set; }
        public bool IsLost { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PetPhotoResponse> Photos { get; set; } = new();
    }
}
