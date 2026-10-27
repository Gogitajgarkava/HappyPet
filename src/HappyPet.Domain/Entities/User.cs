using HappyPet.Domain.Common;
using HappyPet.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyPet.Domain.Entities
{
    public class User : BaseEntity
    {
        public string PersonalId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public Enums.UserRole Role { get; set; } = Enums.UserRole.PetOwner;
        public bool IsEmailVerified { get; set; } = false;
        public DateTime? EmailVerifiedAt { get; set; }
        public string? Address { get; set; }

        public ICollection<Pet> Pets { get; set; } = new List<Pet>();
        public ICollection<UserOtpCode> OtpCodes { get; set; } = new List<UserOtpCode>();
    }
}
