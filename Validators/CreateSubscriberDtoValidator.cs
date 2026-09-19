using FluentValidation;
using FoodMart.Dtos.SubscriberDtos;

namespace FoodMart.Validators;

public sealed class CreateSubscriberDtoValidator : AbstractValidator<CreateSubscriberDto>
{
    public CreateSubscriberDtoValidator()
    {
        RuleFor(x => x.FullName).RequiredText();
        RuleFor(x => x.Email).RequiredText(254).EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
    }
}
