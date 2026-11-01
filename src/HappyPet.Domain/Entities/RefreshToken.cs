using HappyPet.Domain.Common;

namespace HappyPet.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public int UserId { get; set; }

        public string TokenHash { get; set; } = string.Empty;

        public Guid FamilyId { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? ReplacedByTokenHash { get; set; }

        public string? RevocationReason { get; set; }

        public User User { get; set; } = null!;
    }
}
