using FoodMart.Dtos.ProductDtos;
using FoodMart.Services.ProductServices;
using FoodMart.Services.CategoryServices;
using MongoDB.Bson;
using Microsoft.AspNetCore.Mvc;

namespace FoodMart.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();
            products = products.Where(x => x.IsActive).ToList();
            return View(products);
        }


        [HttpGet]
        public async Task<IActionResult> Category(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return RedirectToAction(nameof(Index));

            var category = await _categoryService.GetByIdAsync(id);
            if (category is null) return NotFound();
            var products = await _productService.GetProductsByCategoryAsync(id);

            ViewBag.CategoryId = id;

            ViewBag.CategoryName = category.Name;

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? search, string? categoryId)
        {
            if (!string.IsNullOrWhiteSpace(categoryId) && !ObjectId.TryParse(categoryId, out _)) return NotFound();
            var products =
                string.IsNullOrWhiteSpace(search)
                    ? await _productService.GetAllAsync()
                    : await _productService.SearchProductsAsync(search);

            products = products.Where(x => x.IsActive).ToList();

            if (!string.IsNullOrWhiteSpace(categoryId))
            {
                products = products.Where(x => x.CategoryId == categoryId).ToList();
            }

            ViewBag.SearchTerm = search?.Trim() ?? string.Empty;

            ViewBag.CategoryId = categoryId ?? string.Empty;

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var product = await _productService.GetByIdAsync(id);

            if (product is null || !product.IsActive)
                return NotFound();

            var category = await _categoryService.GetByIdAsync(product.CategoryId);
            ViewBag.CategoryName = category?.Name ?? "Kategori";
            return View(product);
        }
    }
}
