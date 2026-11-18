using FluentValidation;
using HappyPet.Application.Dtos.Requests;

namespace HappyPet.Application.Validators
{
    public class CreateAdoptionListingRequestValidator : AbstractValidator<CreateAdoptionListingRequest>
    {
        public CreateAdoptionListingRequestValidator()
        {
            RuleFor(x => x.PetId)
                .GreaterThan(0).WithMessage("ცხოველის იდენტიფიკატორი დადებითი უნდა იყოს.");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("აღწერა არ უნდა აღემატებოდეს 2000 სიმბოლოს.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("ქალაქი სავალდებულოა.")
                .MaximumLength(100).WithMessage("ქალაქი არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.AdoptionFee)
                .GreaterThanOrEqualTo(0).WithMessage("მიშვილების საფასური უარყოფითი არ შეიძლება იყოს.")
                .LessThanOrEqualTo(100_000).WithMessage("მიშვილების საფასური ძალიან მაღალია.")
                .When(x => x.AdoptionFee.HasValue);
        }
    }

    public class UpdateAdoptionListingRequestValidator : AbstractValidator<UpdateAdoptionListingRequest>
    {
        public UpdateAdoptionListingRequestValidator()
        {
            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("აღწერა არ უნდა აღემატებოდეს 2000 სიმბოლოს.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("ქალაქი სავალდებულოა.")
                .MaximumLength(100).WithMessage("ქალაქი არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.AdoptionFee)
                .GreaterThanOrEqualTo(0).WithMessage("მიშვილების საფასური უარყოფითი არ შეიძლება იყოს.")
                .LessThanOrEqualTo(100_000).WithMessage("მიშვილების საფასური ძალიან მაღალია.")
                .When(x => x.AdoptionFee.HasValue);
        }
    }

    public class CreateAdoptionApplicationRequestValidator : AbstractValidator<CreateAdoptionApplicationRequest>
    {
        public CreateAdoptionApplicationRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("სახელი და გვარი სავალდებულოა.")
                .MaximumLength(100).WithMessage("სახელი და გვარი არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("ელ-ფოსტა სავალდებულოა.")
                .EmailAddress().WithMessage("ელ-ფოსტის ფორმატი არასწორია.")
                .MaximumLength(100).WithMessage("ელ-ფოსტა არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("ტელეფონის ნომერი სავალდებულოა.")
                .MaximumLength(20).WithMessage("ტელეფონის ნომერი არ უნდა აღემატებოდეს 20 სიმბოლოს.")
                .Matches(@"^\+?[0-9\s\-()]{7,20}$").WithMessage("ტელეფონის ნომრის ფორმატი არასწორია.");

            RuleFor(x => x.Address)
                .MaximumLength(300).WithMessage("მისამართი არ უნდა აღემატებოდეს 300 სიმბოლოს.");

            RuleFor(x => x.Message)
                .MaximumLength(2000).WithMessage("განაცხადის ტექსტი არ უნდა აღემატებოდეს 2000 სიმბოლოს.");
        }
    }

    public class ReviewAdoptionApplicationRequestValidator : AbstractValidator<ReviewAdoptionApplicationRequest>
    {
        public ReviewAdoptionApplicationRequestValidator()
        {
            RuleFor(x => x.Note)
                .MaximumLength(500).WithMessage("შენიშვნა არ უნდა აღემატებოდეს 500 სიმბოლოს.");
        }
    }
}
