using FoodMart.Services.DiscountServices;
using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class DiscountComponentPartial : ViewComponent
    {
        private readonly IDiscountService _discountService;

        public DiscountComponentPartial(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var discounts = await _discountService.GetActiveDiscountsAsync(2);

            return View(discounts);
        }
    }
}