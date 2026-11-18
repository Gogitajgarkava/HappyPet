using HappyPet.Domain.Enums;

namespace HappyPet.Application.Dtos.Responses
{
    public class AdoptionListingResponse
    {
        public int Id { get; set; }
        public int ShelterId { get; set; }
        public ShelterSummaryResponse Shelter { get; set; } = new();

        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public PetSpecies Species { get; set; }
        public string? Breed { get; set; }
        public PetGender Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public int? AgeMonths { get; set; }
        public string? Color { get; set; }
        public PetSize? Size { get; set; }
        public bool IsSterilized { get; set; }

        public string? Description { get; set; }
        public string City { get; set; } = string.Empty;
        public decimal? AdoptionFee { get; set; }
        public string Currency { get; set; } = "GEL";
        public bool IsUrgent { get; set; }

        public AdoptionListingStatus Status { get; set; }
        public DateTime PublishedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<PetPhotoResponse> Photos { get; set; } = new();
    }

    public class AdoptionApplicationResponse
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public int ShelterId { get; set; }
        public string ShelterName { get; set; } = string.Empty;
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;

        public int ApplicantUserId { get; set; }
        public string ApplicantFullName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Message { get; set; }

        public AdoptionApplicationStatus Status { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int? ReviewedByUserId { get; set; }
        public string? DecisionNote { get; set; }
        public DateTime? WithdrawnAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
