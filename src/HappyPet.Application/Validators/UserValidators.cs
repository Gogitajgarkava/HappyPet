using FluentValidation;
using HappyPet.Application.Dtos.Requests;

namespace HappyPet.Application.Validators
{
    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.PersonalId)
                .NotEmpty().WithMessage("პირადი ნომერი სავალდებულოა.")
                .Length(11).WithMessage("პირადი ნომერი უნდა შედგებოდეს ზუსტად 11 ციფრისგან.")
                .Matches(@"^\d{11}$").WithMessage("პირადი ნომერი უნდა შედგებოდეს ზუსტად 11 ციფრისგან.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("სახელი სავალდებულოა.")
                .MaximumLength(50).WithMessage("სახელი არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("გვარი სავალდებულოა.")
                .MaximumLength(50).WithMessage("გვარი არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("დაბადების თარიღი სავალდებულოა.")
                .LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("დაბადების თარიღი მომავალში არ შეიძლება იყოს.")
                .GreaterThan(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .WithMessage("დაბადების თარიღი არასწორია.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("ელ-ფოსტა სავალდებულოა.")
                .MaximumLength(100).WithMessage("ელ-ფოსტა არ უნდა აღემატებოდეს 100 სიმბოლოს.")
                .EmailAddress().WithMessage("არასწორი ელ-ფოსტის ფორმატი.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("პაროლი სავალდებულოა.")
                .MinimumLength(6).WithMessage("პაროლი უნდა იყოს მინიმუმ 6 სიმბოლო.")
                .MaximumLength(200).WithMessage("პაროლი არ უნდა აღემატებოდეს 200 სიმბოლოს.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("ტელეფონის ნომერი სავალდებულოა.")
                .MaximumLength(20).WithMessage("ტელეფონის ნომერი არ უნდა აღემატებოდეს 20 სიმბოლოს.");

            RuleFor(x => x.Address)
                .MaximumLength(200).WithMessage("მისამართი არ უნდა აღემატებოდეს 200 სიმბოლოს.");
        }
    }

    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("სახელი სავალდებულოა.")
                .MaximumLength(50).WithMessage("სახელი არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("გვარი სავალდებულოა.")
                .MaximumLength(50).WithMessage("გვარი არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("ტელეფონის ნომერი სავალდებულოა.")
                .MaximumLength(20).WithMessage("ტელეფონის ნომერი არ უნდა აღემატებოდეს 20 სიმბოლოს.");

            RuleFor(x => x.Address)
                .MaximumLength(200).WithMessage("მისამართი არ უნდა აღემატებოდეს 200 სიმბოლოს.");
        }
    }
}
