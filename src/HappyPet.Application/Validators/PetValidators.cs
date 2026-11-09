using FluentValidation;
using HappyPet.Application.Dtos.Requests;

namespace HappyPet.Application.Validators
{
    public class CreatePetRequestValidator : AbstractValidator<CreatePetRequest>
    {
        public CreatePetRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("სახელი სავალდებულოა.")
                .MaximumLength(50).WithMessage("სახელი არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.Species)
                .IsInEnum().WithMessage("სახეობა არასწორია.");

            RuleFor(x => x.Breed)
                .MaximumLength(50).WithMessage("ჯიში არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.MicrochipNumber)
                .MaximumLength(15).WithMessage("მიკროჩიპის ნომერი არ უნდა აღემატებოდეს 15 სიმბოლოს.");

            RuleFor(x => x.BirthDate)
                .LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("დაბადების თარიღი მომავალში არ შეიძლება იყოს.")
                .When(x => x.BirthDate.HasValue);

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("სქესი არასწორია.");

            RuleFor(x => x.Color)
                .MaximumLength(50).WithMessage("ფერი არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.DistinguishingMarks)
                .MaximumLength(500).WithMessage("განმასხვავებელი ნიშნები არ უნდა აღემატებოდეს 500 სიმბოლოს.");
        }
    }

    public class UpdatePetRequestValidator : AbstractValidator<UpdatePetRequest>
    {
        public UpdatePetRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("სახელი სავალდებულოა.")
                .MaximumLength(50).WithMessage("სახელი არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.Species)
                .IsInEnum().WithMessage("სახეობა არასწორია.");

            RuleFor(x => x.Breed)
                .MaximumLength(50).WithMessage("ჯიში არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.MicrochipNumber)
                .NotEmpty().WithMessage("მიკროჩიპის ნომერი სავალდებულოა.")
                .MaximumLength(15).WithMessage("მიკროჩიპის ნომერი არ უნდა აღემატებოდეს 15 სიმბოლოს.");

            RuleFor(x => x.BirthDate)
                .LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("დაბადების თარიღი მომავალში არ შეიძლება იყოს.")
                .When(x => x.BirthDate.HasValue);

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("სქესი არასწორია.");

            RuleFor(x => x.Color)
                .MaximumLength(50).WithMessage("ფერი არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.DistinguishingMarks)
                .MaximumLength(500).WithMessage("განმასხვავებელი ნიშნები არ უნდა აღემატებოდეს 500 სიმბოლოს.");
        }
    }

    public class ReportLostPetRequestValidator : AbstractValidator<ReportLostPetRequest>
    {
        public ReportLostPetRequestValidator()
        {
            RuleFor(x => x.LastSeenAt)
                .NotEmpty().WithMessage("ბოლო ნახვის დრო სავალდებულოა.")
                .LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("ბოლო ნახვის დრო მომავალში არ შეიძლება იყოს.");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("განედი უნდა იყოს -90-დან 90-მდე.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("გრძედი უნდა იყოს -180-დან 180-მდე.");

            RuleFor(x => x.LastSeenAddress)
                .MaximumLength(300).WithMessage("მისამართი არ უნდა აღემატებოდეს 300 სიმბოლოს.");

            RuleFor(x => x.ContactPhone)
                .MaximumLength(20).WithMessage("საკონტაქტო ნომერი არ უნდა აღემატებოდეს 20 სიმბოლოს.")
                .Matches(@"^\+?[0-9\s\-()]{7,20}$")
                .WithMessage("საკონტაქტო ნომრის ფორმატი არასწორია.")
                .When(x => !string.IsNullOrWhiteSpace(x.ContactPhone));
        }
    }
}
