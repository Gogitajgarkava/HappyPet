using HappyPet.Domain.Common;
using HappyPet.Domain.Enums;

namespace HappyPet.Domain.Entities
{
    public class UserOtpCode : BaseEntity
    {
        public int UserId { get; set; }
        public OtpPurpose Purpose { get; set; }
        public string CodeHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime? ConsumedAt { get; set; }
        public int AttemptCount { get; set; }
        public User User { get; set; } = null!;
    }
}
