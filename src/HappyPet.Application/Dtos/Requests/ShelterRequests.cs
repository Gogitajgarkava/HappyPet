namespace HappyPet.Application.Dtos.Requests
{
    /// <summary>
    /// Creates or replaces the authenticated shelter's organisation profile.
    /// Verification is controlled only by an administrator.
    /// </summary>
    public class UpdateShelterProfileRequest
    {
        public string OrganizationName { get; set; } = string.Empty;
        public string? RegistrationNumber { get; set; }
        public string? Description { get; set; }
        public string City { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Website { get; set; }

        /// <summary>Null keeps the current value; on first creation it defaults to true.</summary>
        public bool? IsAcceptingApplications { get; set; }
    }
}
