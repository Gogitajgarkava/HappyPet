using FluentValidation;
using HappyPet.Application.Dtos.Requests;
using HappyPet.Domain.Enums;

namespace HappyPet.Application.Validators
{
    public class CreateMarketplaceListingRequestValidator : AbstractValidator<CreateMarketplaceListingRequest>
    {
        public CreateMarketplaceListingRequestValidator()
        {
            RuleFor(x => x.PetId)
                .GreaterThan(0).WithMessage("ცხოველის იდენტიფიკატორი დადებითი უნდა იყოს.")
                .When(x => x.PetId.HasValue);

            RuleFor(x => x.PetName)
                .NotEmpty().WithMessage("ცხოველის სახელი სავალდებულოა.")
                .MaximumLength(50).WithMessage("ცხოველის სახელი არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.Species).IsInEnum().WithMessage("სახეობა არასწორია.");
            RuleFor(x => x.Gender).IsInEnum().WithMessage("სქესი არასწორია.");

            RuleFor(x => x.Breed)
                .MaximumLength(50).WithMessage("ჯიში არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("დაბადების თარიღი სავალდებულოა.")
                .LessThanOrEqualTo(_ => DateTime.UtcNow.Date.AddDays(-56))
                .WithMessage("განცხადების გამოქვეყნებისთვის ცხოველის ასაკი უნდა იყოს მინიმუმ 8 კვირა.");

            RuleFor(x => x.Color)
                .MaximumLength(50).WithMessage("ფერი არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.Size)
                .IsInEnum().WithMessage("ზომა არასწორია.")
                .When(x => x.Size.HasValue);

            RuleFor(x => x.MicrochipNumber)
                .Matches(@"^\d{15}$").WithMessage("მიკროჩიპის ნომერი უნდა შედგებოდეს 15 ციფრისგან.")
                .When(x => !string.IsNullOrWhiteSpace(x.MicrochipNumber));

            RuleFor(x => x.ListingType).IsInEnum().WithMessage("განცხადების ტიპი არასწორია.");

            RuleFor(x => x.Price)
                .NotNull().WithMessage("გასაყიდი ცხოველის ფასი სავალდებულოა.")
                .GreaterThan(0).WithMessage("გასაყიდი ცხოველის ფასი უნდა იყოს დადებითი.")
                .LessThanOrEqualTo(1_000_000).WithMessage("ფასი არ უნდა აღემატებოდეს 1 000 000 ლარს.")
                .When(x => x.ListingType == ListingType.Sale);

            RuleFor(x => x.Price)
                .Must(price => !price.HasValue || price.Value == 0)
                .WithMessage("უსასყიდლოდ გადასაცემი ცხოველისთვის ფასი არ უნდა იყოს მითითებული.")
                .When(x => x.ListingType == ListingType.Giveaway);

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("ქალაქი სავალდებულოა.")
                .MaximumLength(100).WithMessage("ქალაქი არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.ContactPhone)
                .NotEmpty().WithMessage("საკონტაქტო ნომერი სავალდებულოა.")
                .MaximumLength(20).WithMessage("საკონტაქტო ნომერი არ უნდა აღემატებოდეს 20 სიმბოლოს.")
                .Matches(@"^\+?[0-9\s\-()]{7,20}$").WithMessage("საკონტაქტო ნომრის ფორმატი არასწორია.");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("აღწერა არ უნდა აღემატებოდეს 2000 სიმბოლოს.");
        }
    }

    public class UpdateMarketplaceListingRequestValidator : AbstractValidator<UpdateMarketplaceListingRequest>
    {
        public UpdateMarketplaceListingRequestValidator()
        {
            RuleFor(x => x.PetId)
                .GreaterThan(0).WithMessage("ცხოველის იდენტიფიკატორი დადებითი უნდა იყოს.")
                .When(x => x.PetId.HasValue);

            RuleFor(x => x.PetName)
                .NotEmpty().WithMessage("ცხოველის სახელი სავალდებულოა.")
                .MaximumLength(50).WithMessage("ცხოველის სახელი არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.Species).IsInEnum().WithMessage("სახეობა არასწორია.");
            RuleFor(x => x.Gender).IsInEnum().WithMessage("სქესი არასწორია.");

            RuleFor(x => x.Breed)
                .MaximumLength(50).WithMessage("ჯიში არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("დაბადების თარიღი სავალდებულოა.")
                .LessThanOrEqualTo(_ => DateTime.UtcNow.Date.AddDays(-56))
                .WithMessage("განცხადების გამოქვეყნებისთვის ცხოველის ასაკი უნდა იყოს მინიმუმ 8 კვირა.");

            RuleFor(x => x.Color)
                .MaximumLength(50).WithMessage("ფერი არ უნდა აღემატებოდეს 50 სიმბოლოს.");

            RuleFor(x => x.Size)
                .IsInEnum().WithMessage("ზომა არასწორია.")
                .When(x => x.Size.HasValue);

            RuleFor(x => x.MicrochipNumber)
                .Matches(@"^\d{15}$").WithMessage("მიკროჩიპის ნომერი უნდა შედგებოდეს 15 ციფრისგან.")
                .When(x => !string.IsNullOrWhiteSpace(x.MicrochipNumber));

            RuleFor(x => x.ListingType).IsInEnum().WithMessage("განცხადების ტიპი არასწორია.");

            RuleFor(x => x.Price)
                .NotNull().WithMessage("გასაყიდი ცხოველის ფასი სავალდებულოა.")
                .GreaterThan(0).WithMessage("გასაყიდი ცხოველის ფასი უნდა იყოს დადებითი.")
                .LessThanOrEqualTo(1_000_000).WithMessage("ფასი არ უნდა აღემატებოდეს 1 000 000 ლარს.")
                .When(x => x.ListingType == ListingType.Sale);

            RuleFor(x => x.Price)
                .Must(price => !price.HasValue || price.Value == 0)
                .WithMessage("უსასყიდლოდ გადასაცემი ცხოველისთვის ფასი არ უნდა იყოს მითითებული.")
                .When(x => x.ListingType == ListingType.Giveaway);

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("ქალაქი სავალდებულოა.")
                .MaximumLength(100).WithMessage("ქალაქი არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.ContactPhone)
                .NotEmpty().WithMessage("საკონტაქტო ნომერი სავალდებულოა.")
                .MaximumLength(20).WithMessage("საკონტაქტო ნომერი არ უნდა აღემატებოდეს 20 სიმბოლოს.")
                .Matches(@"^\+?[0-9\s\-()]{7,20}$").WithMessage("საკონტაქტო ნომრის ფორმატი არასწორია.");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("აღწერა არ უნდა აღემატებოდეს 2000 სიმბოლოს.");
        }
    }

    public class CompleteMarketplaceListingRequestValidator : AbstractValidator<CompleteMarketplaceListingRequest>
    {
        public CompleteMarketplaceListingRequestValidator()
        {
            RuleFor(x => x.BuyerUserId)
                .GreaterThan(0).WithMessage("მყიდველის იდენტიფიკატორი დადებითი უნდა იყოს.")
                .When(x => x.BuyerUserId.HasValue);

            RuleFor(x => x.BuyerFullName)
                .NotEmpty().WithMessage("მყიდველის სახელი და გვარი სავალდებულოა.")
                .MaximumLength(100).WithMessage("მყიდველის სახელი და გვარი არ უნდა აღემატებოდეს 100 სიმბოლოს.");

            RuleFor(x => x.BuyerPersonalId)
                .NotEmpty().WithMessage("მყიდველის პირადი ნომერი სავალდებულოა.")
                .Matches(@"^\d{11}$").WithMessage("მყიდველის პირადი ნომერი უნდა შედგებოდეს 11 ციფრისგან.");

            RuleFor(x => x.BuyerPhone)
                .MaximumLength(20).WithMessage("მყიდველის ტელეფონი არ უნდა აღემატებოდეს 20 სიმბოლოს.")
                .Matches(@"^\+?[0-9\s\-()]{7,20}$").WithMessage("მყიდველის ტელეფონის ფორმატი არასწორია.")
                .When(x => !string.IsNullOrWhiteSpace(x.BuyerPhone));

            RuleFor(x => x.TransferredAt)
                .LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("გადაცემის დრო მომავალში არ შეიძლება იყოს.")
                .When(x => x.TransferredAt.HasValue);

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("შენიშვნა არ უნდა აღემატებოდეს 500 სიმბოლოს.");
        }
    }

    public class BlockMarketplaceListingRequestValidator : AbstractValidator<BlockMarketplaceListingRequest>
    {
        public BlockMarketplaceListingRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("დაბლოკვის მიზეზი სავალდებულოა.")
                .MaximumLength(500).WithMessage("დაბლოკვის მიზეზი არ უნდა აღემატებოდეს 500 სიმბოლოს.");
        }
    }
}
