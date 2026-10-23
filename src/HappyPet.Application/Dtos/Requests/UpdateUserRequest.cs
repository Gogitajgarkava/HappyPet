using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyPet.Application.Dtos.Requests
{
    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "სახელი სავალდებულოა.")]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "გვარი სავალდებულოა.")]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ტელეფონის ნომერი სავალდებულოა.")]
        [Phone(ErrorMessage = "არასწორი ტელეფონის ნომრის ფორმატი.")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? Address { get; set; }


    }
}
