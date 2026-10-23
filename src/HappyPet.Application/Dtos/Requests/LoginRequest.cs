using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyPet.Application.Dtos.Requests;

public class LoginRequest
{
    [Required(ErrorMessage = "ელ-ფოსტის მითითება სავალდებულოა.")]
    [EmailAddress(ErrorMessage = "არასწორი ელ-ფოსტის ფორმატი.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "პაროლის მითითება სავალდებულოა.")]
    public string Password { get; set; } = string.Empty;
}
