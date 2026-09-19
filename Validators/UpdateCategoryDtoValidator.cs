using FluentValidation;
using FoodMart.Dtos.CategoryDtos;

namespace FoodMart.Validators;

public sealed class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator()
    {
        RuleFor(x => x.Id).MongoId();
        RuleFor(x => x.Name).RequiredText();
        RuleFor(x => x.Icon).RequiredText(100);
    }
}
