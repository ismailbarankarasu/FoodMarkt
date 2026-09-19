using FluentValidation;
using FoodMart.Dtos.FeatureDtos;

namespace FoodMart.Validators;

public sealed class UpdateFeatureDtoValidator : AbstractValidator<UpdateFeatureDto>
{
    public UpdateFeatureDtoValidator()
    {
        RuleFor(x => x.Id).MongoId();
        RuleFor(x => x.Description).MaximumLength(4000).WithMessage("Açıklama en fazla 4000 karakter olabilir.");
        RuleFor(x => x.ImageUrl).Must(FormRules.SafeUrl).WithMessage("Geçerli bir görsel URL giriniz veya görsel yükleyiniz.").When(x => x.ImageFile == null && !string.IsNullOrWhiteSpace(x.ImageUrl));
        RuleFor(x => x.Title).RequiredText();
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0).WithMessage("Sıra negatif olamaz.");
        RuleFor(x => x.ButtonText).RequiredText(80);
        RuleFor(x => x.ButtonUrl).Must(FormRules.SafeUrl).WithMessage("Geçerli bir bağlantı giriniz.");
    }
}
