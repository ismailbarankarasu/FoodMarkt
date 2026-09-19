using FluentValidation;
using FoodMart.Entities;
using MongoDB.Driver;
using MongoDB.Bson;
using FoodMart.Dtos.DiscountDtos;

namespace FoodMart.Validators;

public sealed class CreateDiscountDtoValidator : AbstractValidator<CreateDiscountDto>
{
    public CreateDiscountDtoValidator(IMongoDatabase database)
    {
        RuleFor(x => x.Description).MaximumLength(4000).WithMessage("Açıklama en fazla 4000 karakter olabilir.");
        RuleFor(x => x.ImageUrl).Must(FormRules.SafeUrl).WithMessage("Geçerli bir görsel URL giriniz veya görsel yükleyiniz.").When(x => x.ImageFile == null);
        RuleFor(x => x.Title).RequiredText();
        RuleFor(x => x.ProductId).MongoId();
        RuleFor(x => x.ProductId).MustAsync(async (id, token) =>
            !ObjectId.TryParse(id, out _) || await database.GetCollection<Product>("Products").Find(x => x.Id == id).AnyAsync(token))
            .WithMessage("Seçilen kayıt bulunamadı.");
        RuleFor(x => x.DiscountRate).InclusiveBetween(1,100).WithMessage("İndirim oranı 1 ile 100 arasında olmalıdır.");
        RuleFor(x => x.StartDate).NotEmpty().WithMessage("Başlangıç tarihi zorunludur.");
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
    }
}
