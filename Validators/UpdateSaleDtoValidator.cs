using FluentValidation;
using FoodMart.Entities;
using MongoDB.Driver;
using MongoDB.Bson;
using FoodMart.Dtos.SaleDtos;

namespace FoodMart.Validators;

public sealed class UpdateSaleDtoValidator : AbstractValidator<UpdateSaleDto>
{
    public UpdateSaleDtoValidator(IMongoDatabase database)
    {
        RuleFor(x => x.Id).MongoId();
        RuleFor(x => x.ProductId).MongoId();
        RuleFor(x => x.ProductId).MustAsync(async (id, token) =>
            !ObjectId.TryParse(id, out _) || await database.GetCollection<Product>("Products").Find(x => x.Id == id).AnyAsync(token))
            .WithMessage("Seçilen kayıt bulunamadı.");
        RuleFor(x => x.Quantity).InclusiveBetween(1,1000000).WithMessage("Adet 1 ile 1.000.000 arasında olmalıdır.");
        RuleFor(x => x.UnitPrice).GreaterThan(0).WithMessage("Birim fiyat sıfırdan büyük olmalıdır.").LessThanOrEqualTo(10000000).WithMessage("Birim fiyat en fazla 10.000.000 olabilir.");
        RuleFor(x => x.SaleDate).NotEmpty().WithMessage("Satış tarihi zorunludur.");
    }
}
