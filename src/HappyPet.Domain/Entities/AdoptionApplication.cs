using HappyPet.Domain.Common;
using HappyPet.Domain.Enums;

namespace HappyPet.Domain.Entities
{
    /// <summary>
    /// An application submitted by a registered user for an adoption listing.
    /// A user can have at most one application per listing.
    /// </summary>
    public class AdoptionApplication : BaseEntity
    {
        public int ListingId { get; set; }
        public int ApplicantUserId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Message { get; set; }

        public AdoptionApplicationStatus Status { get; set; } = AdoptionApplicationStatus.Pending;
        public DateTime? ReviewedAt { get; set; }
        public int? ReviewedByUserId { get; set; }
        public string? DecisionNote { get; set; }
        public DateTime? WithdrawnAt { get; set; }

        public AdoptionListing Listing { get; set; } = null!;
        public User Applicant { get; set; } = null!;
    }
}
