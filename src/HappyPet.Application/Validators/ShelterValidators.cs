using FluentValidation;
using HappyPet.Application.Dtos.Requests;

namespace HappyPet.Application.Validators
{
    public class UpdateShelterProfileRequestValidator : AbstractValidator<UpdateShelterProfileRequest>
    {
        public UpdateShelterProfileRequestValidator()
        {
            RuleFor(x => x.OrganizationName)
                .NotEmpty().WithMessage("ორგანიზაციის სახელი სავალდებულოა.")
                .MaximumLength(200).WithMessage("ორგანიზაციის სახელი არ უნდა აღემატებოდეს 200 სიმბოლოს.");

            RuleFor(x => x.RegistrationNumber)
                .MaximumLength(50).WithMessage("სარეგისტრაციო ნომერი არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("აღწერა არ უნდა აღემატებოდეს 2000 სიმბოლოს.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("ქალაქი სავალდებულოა.")
                .MaximumLength(100).WithMessage("ქალაქი არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.Address)
                .MaximumLength(300).WithMessage("მისამართი არ უნდა აღემატებოდეს 300 სიმბოლოს.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("საკონტაქტო ნომერი სავალდებულოა.")
                .MaximumLength(20).WithMessage("საკონტაქტო ნომერი არ უნდა აღემატებოდეს 20 სიმბოლოს.")
                .Matches(@"^\+?[0-9\s\-()]{7,20}$").WithMessage("საკონტაქტო ნომრის ფორმატი არასწორია.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("ელ-ფოსტის ფორმატი არასწორია.")
                .MaximumLength(100).WithMessage("ელ-ფოსტა არ უნდა აღემატებოდეს 100 სიმბოლოს.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Website)
                .MaximumLength(200).WithMessage("ვებ-გვერდი არ უნდა აღემატებოდეს 200 სიმბოლოს.")
                .Must(BeValidWebsite).WithMessage("ვებ-გვერდი უნდა იყოს სწორი http ან https მისამართი.")
                .When(x => !string.IsNullOrWhiteSpace(x.Website));
        }

        private static bool BeValidWebsite(string? website)
        {
            return Uri.TryCreate(website, UriKind.Absolute, out var uri)
                   && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
