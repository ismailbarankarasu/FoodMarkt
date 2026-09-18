using FoodMart.Services.ProductServices;
using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class LatestProductsComponentPartial : ViewComponent
    {
        private readonly IProductService _productService;

        public LatestProductsComponentPartial(
            IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var products = await _productService.GetLatestProductsAsync(10);

            return View(products);
        }
    }
}