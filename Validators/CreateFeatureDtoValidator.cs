using FluentValidation;
using FoodMart.Dtos.FeatureDtos;

namespace FoodMart.Validators;

public sealed class CreateFeatureDtoValidator : AbstractValidator<CreateFeatureDto>
{
    public CreateFeatureDtoValidator()
    {
        RuleFor(x => x.Description).MaximumLength(4000).WithMessage("Açıklama en fazla 4000 karakter olabilir.");
        RuleFor(x => x.ImageUrl).Must(FormRules.SafeUrl).WithMessage("Geçerli bir görsel URL giriniz veya görsel yükleyiniz.").When(x => x.ImageFile == null);
        RuleFor(x => x.Title).RequiredText();
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0).WithMessage("Sıra negatif olamaz.");
        RuleFor(x => x.ButtonText).RequiredText(80);
        RuleFor(x => x.ButtonUrl).Must(FormRules.SafeUrl).WithMessage("Geçerli bir bağlantı giriniz.");
    }
}
