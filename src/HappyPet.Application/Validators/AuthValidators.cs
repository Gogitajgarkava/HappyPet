using FluentValidation;
using HappyPet.Application.Dtos.Requests;

namespace HappyPet.Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("ელ-ფოსტის მითითება სავალდებულოა.")
                .MaximumLength(100).WithMessage("ელ-ფოსტა არ უნდა აღემატებოდეს 100 სიმბოლოს.")
                .EmailAddress().WithMessage("არასწორი ელ-ფოსტის ფორმატი.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("პაროლის მითითება სავალდებულოა.")
                .MaximumLength(200).WithMessage("პაროლი არ უნდა აღემატებოდეს 200 სიმბოლოს.");
        }
    }

    public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
    {
        public VerifyEmailRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("ელ-ფოსტა სავალდებულოა.")
                .EmailAddress().WithMessage("არასწორი ელ-ფოსტის ფორმატი.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("კოდი სავალდებულოა.")
                .Matches(@"^\d{6}$").WithMessage("კოდი უნდა შედგებოდეს ზუსტად 6 ციფრისგან.");
        }
    }

    public class ResendVerificationRequestValidator : AbstractValidator<ResendVerificationRequest>
    {
        public ResendVerificationRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("ელ-ფოსტა სავალდებულოა.")
                .EmailAddress().WithMessage("არასწორი ელ-ფოსტის ფორმატი.");
        }
    }

    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("ელ-ფოსტა სავალდებულოა.")
                .EmailAddress().WithMessage("არასწორი ელ-ფოსტის ფორმატი.");
        }
    }

    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("ელ-ფოსტა სავალდებულოა.")
                .EmailAddress().WithMessage("არასწორი ელ-ფოსტის ფორმატი.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("კოდი სავალდებულოა.")
                .Matches(@"^\d{6}$").WithMessage("კოდი უნდა შედგებოდეს ზუსტად 6 ციფრისგან.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("ახალი პაროლი სავალდებულოა.")
                .MinimumLength(6).WithMessage("პაროლი უნდა იყოს მინიმუმ 6 სიმბოლო.")
                .MaximumLength(200).WithMessage("პაროლი არ უნდა აღემატებოდეს 200 სიმბოლოს.");
        }
    }
}
