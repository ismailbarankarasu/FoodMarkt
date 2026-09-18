using FoodMart.Services.SaleServices;
using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class PopularProductsComponentPartial : ViewComponent
    {
        private readonly ISaleService _saleService;

        public PopularProductsComponentPartial(
            ISaleService saleService)
        {
            _saleService = saleService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var products = await _saleService.GetPopularProductsAsync(6);

            return View(products);
        }
    }
}