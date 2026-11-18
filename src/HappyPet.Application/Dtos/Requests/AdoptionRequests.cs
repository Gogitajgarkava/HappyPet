namespace HappyPet.Application.Dtos.Requests
{
    public class CreateAdoptionListingRequest
    {
        /// <summary>A pet registered to the authenticated shelter.</summary>
        public int PetId { get; set; }

        public string? Description { get; set; }
        public string City { get; set; } = string.Empty;
        public decimal? AdoptionFee { get; set; }
        public bool IsUrgent { get; set; }
    }

    public class UpdateAdoptionListingRequest
    {
        public string? Description { get; set; }
        public string City { get; set; } = string.Empty;
        public decimal? AdoptionFee { get; set; }
        public bool IsUrgent { get; set; }
    }

    public class CreateAdoptionApplicationRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Message { get; set; }
    }

    public class ReviewAdoptionApplicationRequest
    {
        public string? Note { get; set; }
    }
}
