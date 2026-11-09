using FluentValidation;
using HappyPet.Application.Dtos.Requests;

namespace HappyPet.Application.Validators
{
    public class CreateVaccinationRequestValidator : AbstractValidator<CreateVaccinationRequest>
    {
        public CreateVaccinationRequestValidator()
        {
            RuleFor(x => x.VaccineName)
                .NotEmpty().WithMessage("ვაქცინის დასახელება სავალდებულოა.")
                .MaximumLength(100).WithMessage("ვაქცინის დასახელება არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.Disease)
                .MaximumLength(100).WithMessage("დაავადება არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.Manufacturer)
                .MaximumLength(100).WithMessage("მწარმოებელი არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.BatchNumber)
                .MaximumLength(50).WithMessage("სერიის ნომერი არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.CertificateNumber)
                .MaximumLength(50).WithMessage("სერტიფიკატის ნომერი არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.VaccinatedAt)
                .NotEmpty().WithMessage("ვაქცინაციის თარიღი სავალდებულოა.")
                .LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("ვაქცინაციის თარიღი მომავალში არ შეიძლება იყოს.");

            RuleFor(x => x.NextDueAt)
                .GreaterThan(x => x.VaccinatedAt)
                .WithMessage("შემდეგი ვაქცინაციის თარიღი უნდა იყოს ვაქცინაციის თარიღზე გვიან.")
                .When(x => x.NextDueAt.HasValue);

            RuleFor(x => x.VeterinarianName)
                .MaximumLength(100).WithMessage("ვეტერინარის სახელი არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.ClinicName)
                .MaximumLength(100).WithMessage("კლინიკის დასახელება არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("შენიშვნა არ უნდა აღემატებოდეს 500 სიმბოლოს.");
        }
    }

    public class UpdateVaccinationRequestValidator : AbstractValidator<UpdateVaccinationRequest>
    {
        public UpdateVaccinationRequestValidator()
        {
            RuleFor(x => x.VaccineName)
                .NotEmpty().WithMessage("ვაქცინის დასახელება სავალდებულოა.")
                .MaximumLength(100).WithMessage("ვაქცინის დასახელება არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.Disease)
                .MaximumLength(100).WithMessage("დაავადება არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.Manufacturer)
                .MaximumLength(100).WithMessage("მწარმოებელი არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.BatchNumber)
                .MaximumLength(50).WithMessage("სერიის ნომერი არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.CertificateNumber)
                .MaximumLength(50).WithMessage("სერტიფიკატის ნომერი არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.VaccinatedAt)
                .NotEmpty().WithMessage("ვაქცინაციის თარიღი სავალდებულოა.")
                .LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("ვაქცინაციის თარიღი მომავალში არ შეიძლება იყოს.");

            RuleFor(x => x.NextDueAt)
                .GreaterThan(x => x.VaccinatedAt)
                .WithMessage("შემდეგი ვაქცინაციის თარიღი უნდა იყოს ვაქცინაციის თარიღზე გვიან.")
                .When(x => x.NextDueAt.HasValue);

            RuleFor(x => x.VeterinarianName)
                .MaximumLength(100).WithMessage("ვეტერინარის სახელი არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.ClinicName)
                .MaximumLength(100).WithMessage("კლინიკის დასახელება არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("შენიშვნა არ უნდა აღემატებოდეს 500 სიმბოლოს.");
        }
    }
}
