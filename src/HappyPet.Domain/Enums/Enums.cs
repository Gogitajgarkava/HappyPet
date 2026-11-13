using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyPet.Domain.Enums
{
    public enum UserRole
    {
        User = 0,
        PetOwner = 1,
        Veterinarian = 2,
        Shelter = 3,
        Admin = 4

    }
    public enum PetSpecies 
    {
        Dog = 1,
        Cat = 2,
        Other = 3
    }

    public enum PetGender
    {
        Male = 1,
        Female = 2,
        Neutered = 3,
        Spayed = 4
    }

    public enum PetSize
    {
        Small = 1,
        Medium = 2,
        Large = 3
    }

    public enum VaccinationType
    {
        Prophylactic = 1,
        Mandatory = 2
    }

    public enum ListingType
    {
        Sale = 1,
        Giveaway = 2
    }

    public enum ListingStatus
    {
        Active = 1,
        Completed = 2,
        Expired = 3,
        Blocked = 4,
        Withdrawn = 5
    }

    public enum OwnershipTransferType
    {
        Sale = 1,
        Gift = 2
    }

    public enum OwnershipTransferStatus
    {
        PendingAirsSync = 1,
        Synced = 2,
        Rejected = 3
    }

    public enum OtpPurpose
    {
        EmailVerification = 1,
        PasswordReset = 2
    }
}
