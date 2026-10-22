using HappyPet.Domain.Enums;
using System.Net;

namespace HappyPet.Application.Dtos.Responses
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string PersonalId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsVerified { get; set; }
        public string? Address { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}