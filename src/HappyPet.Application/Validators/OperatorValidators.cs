using FluentValidation;
using HappyPet.Application.Dtos.Requests;

namespace HappyPet.Application.Validators
{
    public class MarkOwnershipTransferSyncedRequestValidator
        : AbstractValidator<MarkOwnershipTransferSyncedRequest>
    {
        public MarkOwnershipTransferSyncedRequestValidator()
        {
            RuleFor(x => x.AirsReference)
                .NotEmpty().WithMessage("AIRS-ის მითითების ნომერი სავალდებულოა.")
                .MaximumLength(100).WithMessage("AIRS-ის მითითების ნომერი არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.SyncedAt)
                .LessThanOrEqualTo(_ => DateTime.UtcNow)
                .WithMessage("AIRS-ში სინქრონიზაციის დრო მომავალში არ შეიძლება იყოს.")
                .When(x => x.SyncedAt.HasValue);

            RuleFor(x => x.BuyerUserId)
                .GreaterThan(0).WithMessage("მყიდველის იდენტიფიკატორი დადებითი უნდა იყოს.")
                .When(x => x.BuyerUserId.HasValue);

            RuleFor(x => x.BuyerAddress)
                .MaximumLength(300).WithMessage("მყიდველის მისამართი არ უნდა აღემატებოდეს 300 სიმბოლოს.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("ოპერატორის შენიშვნა არ უნდა აღემატებოდეს 500 სიმბოლოს.");
        }
    }

    public class RejectOwnershipTransferRequestValidator
        : AbstractValidator<RejectOwnershipTransferRequest>
    {
        public RejectOwnershipTransferRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("უარყოფის მიზეზი სავალდებულოა.")
                .MaximumLength(500).WithMessage("უარყოფის მიზეზი არ უნდა აღემატებოდეს 500 სიმბოლოს.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("ოპერატორის შენიშვნა არ უნდა აღემატებოდეს 500 სიმბოლოს.");
        }
    }
}
