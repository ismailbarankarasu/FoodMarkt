using FluentValidation;
using FoodMart.Entities;
using MongoDB.Driver;
using MongoDB.Bson;
using FoodMart.Dtos.ProductDtos;

namespace FoodMart.Validators;

public sealed class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator(IMongoDatabase database)
    {
        RuleFor(x => x.Id).MongoId();
        RuleFor(x => x.Name).RequiredText();
        RuleFor(x => x.Description).MaximumLength(4000).WithMessage("Açıklama en fazla 4000 karakter olabilir.");
        RuleFor(x => x.ImageUrl).Must(FormRules.SafeUrl).WithMessage("Geçerli bir görsel URL giriniz veya görsel yükleyiniz.").When(x => x.ImageFile == null && !string.IsNullOrWhiteSpace(x.ImageUrl));
        RuleFor(x => x.CategoryId).MongoId();
        RuleFor(x => x.CategoryId).MustAsync(async (id, token) =>
            !ObjectId.TryParse(id, out _) || await database.GetCollection<Category>("Categories").Find(x => x.Id == id).AnyAsync(token))
            .WithMessage("Seçilen kayıt bulunamadı.");
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Fiyat sıfırdan büyük olmalıdır.").LessThanOrEqualTo(10000000).WithMessage("Fiyat en fazla 10.000.000 olabilir.");
        RuleFor(x => x.DiscountPrice).GreaterThanOrEqualTo(0).WithMessage("İndirimli fiyat negatif olamaz.").LessThanOrEqualTo(x => x.Price).WithMessage("İndirimli fiyat normal fiyatı aşamaz.");
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0).WithMessage("Stok negatif olamaz.");
    }
}
