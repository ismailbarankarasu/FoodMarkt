using FoodMart.Dtos.CategoryDtos;
using FoodMart.Services.CategoryServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var values = await _categoryService.GetAllAsync();
            return View(values);
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto createCategoryDto)
        {
            if (!ModelState.IsValid) return View(createCategoryDto);
            await _categoryService.CreateAsync(createCategoryDto);

            TempData["AdminSuccess"] = "İşlem başarıyla tamamlandı.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            await _categoryService.DeleteAsync(id);

            TempData["AdminSuccess"] = "İşlem başarıyla tamamlandı.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateCategory(string id)
        {
            var value = await _categoryService.GetByIdAsync(id);

            if (value is null)
                return NotFound();

            var updateCategoryDto = new UpdateCategoryDto
            {
                Id = value.Id,
                Name = value.Name,
                Icon = value.Icon
            };

            return View(updateCategoryDto);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(UpdateCategoryDto updateCategoryDto)
        {
            if (!ModelState.IsValid) return View(updateCategoryDto);
            await _categoryService.UpdateAsync(updateCategoryDto);
            TempData["AdminSuccess"] = "İşlem başarıyla tamamlandı.";
            return RedirectToAction(nameof(Index));
        }
    }
}