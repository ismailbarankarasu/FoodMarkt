using FoodMart.Services.CategoryServices;
using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class _LayoutHeaderComponentPartial : ViewComponent
    {
        private readonly ICategoryService _categoryService;

        public _LayoutHeaderComponentPartial(
            ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _categoryService.GetAllAsync();

            return View(categories);
        }
    }
}