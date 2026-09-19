using FluentValidation;
using MongoDB.Bson;

namespace FoodMart.Validators;

public static class FormRules
{
    public static IRuleBuilderOptions<T, string> RequiredText<T>(this IRuleBuilder<T, string> rule, int length = 150) =>
        rule.NotEmpty().WithMessage("Bu alan zorunludur.").MaximumLength(length).WithMessage($"En fazla {length} karakter giriniz.");

    public static IRuleBuilderOptions<T, string> MongoId<T>(this IRuleBuilder<T, string> rule) =>
        rule.Must(value => ObjectId.TryParse(value, out _)).WithMessage("Lütfen geçerli bir kayıt seçiniz.");

    public static bool SafeUrl(string? value) => !string.IsNullOrWhiteSpace(value) &&
        !value.Any(char.IsControl) && !value.Contains('\\') &&
        ((value.StartsWith('/') && !value.StartsWith("//")) ||
         (value.StartsWith("~/") && !value.StartsWith("~//")) ||
         (Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https"));
}
