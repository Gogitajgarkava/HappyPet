using System.ComponentModel.DataAnnotations;

namespace HappyPet.Application.Dtos.Requests
{
    public class ResendVerificationRequest
    {
        [Required(ErrorMessage = "ელ-ფოსტა სავალდებულოა.")]
        [EmailAddress(ErrorMessage = "არასწორი ელ-ფოსტის ფორმატი.")]
        public string Email { get; set; } = string.Empty;
    }
}
