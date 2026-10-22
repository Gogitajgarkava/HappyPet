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
}
