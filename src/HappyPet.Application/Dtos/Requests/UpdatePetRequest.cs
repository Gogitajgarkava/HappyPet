using HappyPet.Domain.Enums;

namespace HappyPet.Application.Dtos.Requests
{
    public class UpdatePetRequest
    {
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

        public bool IsLost { get; set; } = false;
    }
}
