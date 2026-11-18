using HappyPet.Domain.Common;

namespace HappyPet.Domain.Entities
{
    /// <summary>
    /// Public organisation profile for a user with the Shelter role. A profile can
    /// be created by the shelter itself, but only an administrator can verify it.
    /// Only verified shelters may publish adoption listings.
    /// </summary>
    public class ShelterProfile : BaseEntity
    {
        public int UserId { get; set; }

        public string OrganizationName { get; set; } = string.Empty;
        public string? RegistrationNumber { get; set; }
        public string? Description { get; set; }
        public string City { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Website { get; set; }

        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public int? VerifiedByUserId { get; set; }

        /// <summary>When false, the shelter remains visible but cannot receive new applications.</summary>
        public bool IsAcceptingApplications { get; set; } = true;

        public User User { get; set; } = null!;
    }
}
