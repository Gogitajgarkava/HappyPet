namespace HappyPet.Application.Dtos.Responses
{
    public class ShelterProfileResponse
    {
        public int Id { get; set; }
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
        public bool IsAcceptingApplications { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>Privacy-safe projection used by anonymous shelter endpoints.</summary>
    public class ShelterPublicResponse
    {
        public int Id { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string City { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Website { get; set; }
        public bool IsVerified { get; set; }
        public bool IsAcceptingApplications { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ShelterSummaryResponse
    {
        public int Id { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Website { get; set; }
        public bool IsVerified { get; set; }
        public bool IsAcceptingApplications { get; set; }
    }

    public class ShelterDashboardStats
    {
        public int TotalListings { get; set; }
        public int ActiveListings { get; set; }
        public int PausedListings { get; set; }
        public int AdoptedListings { get; set; }
        public int WithdrawnListings { get; set; }
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int CompletedAdoptions { get; set; }
    }

    public class ShelterDashboardResponse
    {
        public ShelterProfileResponse? Shelter { get; set; }
        public ShelterDashboardStats Stats { get; set; } = new();
    }

    public class AdminShelterDashboardResponse
    {
        public int TotalShelters { get; set; }
        public int VerifiedShelters { get; set; }
        public int UnverifiedShelters { get; set; }
        public int ActiveListings { get; set; }
        public int PendingApplications { get; set; }
        public int CompletedAdoptions { get; set; }
    }
}
