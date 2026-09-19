using FluentValidation;
using FoodMart.Dtos.CategoryDtos;

namespace FoodMart.Validators;

public sealed class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        RuleFor(x => x.Name).RequiredText();
        RuleFor(x => x.Icon).RequiredText(100);
    }
}
