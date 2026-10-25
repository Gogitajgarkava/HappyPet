using HappyPet.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace HappyPet.Application.Dtos.Requests
{
    public class CreatePetRequest
    {
        [Required(ErrorMessage = "სახელი სავალდებულოა.")]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "სახეობა სავალდებულოა.")]
        public PetSpecies Species { get; set; }

        [MaxLength(50)]
        public string? Breed { get; set; }

        [Required(ErrorMessage = "მიკროჩიპის ნომერი სავალდებულოა.")]
        [MaxLength(15, ErrorMessage = "მიკროჩიპის ნომერი არ უნდა აღემატებოდეს 15 სიმბოლოს.")]
        public string MicrochipNumber { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "სქესი სავალდებულოა.")]
        public PetGender Gender { get; set; }

        public string? Color { get; set; }

        public string? DistinguishingMarks { get; set; }

        public bool IsLost { get; set; } = false;
    }
}
