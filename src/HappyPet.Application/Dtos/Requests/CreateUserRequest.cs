using System.ComponentModel.DataAnnotations;

namespace HappyPet.Application.Dtos.Requests
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "პირადი ნომერი სავალდებულოა.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "პირადი ნომერი უნდა შედგებოდეს ზუსტად 11 ციფრისგან.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "პირადი ნომერი უნდა შედგებოდეს ზუსტად 11 ციფრისგან.")]
        public string PersonalId { get; set; } = string.Empty;

        [Required(ErrorMessage = "სახელი სავალდებულოა.")]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "გვარი სავალდებულოა.")]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "დაბადების თარიღი სავალდებულოა.")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "ელ-ფოსტა სავალდებულოა.")]
        [EmailAddress(ErrorMessage = "არასწორი ელ-ფოსტის ფორმატი.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "პაროლი სავალდებულოა.")]
        [MinLength(6, ErrorMessage = "პაროლი უნდა იყოს მცირე 6 სიმბოლო.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "ტელეფონის ნომერი სავალდებულოა.")]
        [Phone(ErrorMessage = "არასწორი ტელეფონის ნომრის ფორმატი.")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? Address { get; set; }
    }
}