using System.ComponentModel.DataAnnotations;

namespace HappyPet.Application.Dtos.Requests
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "ელ-ფოსტა სავალდებულოა.")]
        [EmailAddress(ErrorMessage = "არასწორი ელ-ფოსტის ფორმატი.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "კოდი სავალდებულოა.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "კოდი უნდა შედგებოდეს ზუსტად 6 ციფრისგან.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "ახალი პაროლი სავალდებულოა.")]
        [MinLength(6, ErrorMessage = "პაროლი უნდა იყოს მცირე 6 სიმბოლო.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
