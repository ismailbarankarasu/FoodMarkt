using FluentValidation;
using FoodMart.Dtos.AdminDtos;

namespace FoodMart.Validators;

public sealed class AdminLoginDtoValidator : AbstractValidator<AdminLoginDto>
{
    public AdminLoginDtoValidator()
    {
        RuleFor(x => x.Email).RequiredText(254).EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Şifre zorunludur.").MaximumLength(1024).WithMessage("Şifre çok uzun.");
    }
}
