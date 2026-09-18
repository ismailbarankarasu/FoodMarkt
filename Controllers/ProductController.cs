using FoodMart.Dtos.ProductDtos;
using FoodMart.Services.ProductServices;
using Microsoft.AspNetCore.Mvc;

namespace FoodMart.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
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

            var products = await _productService.GetProductsByCategoryAsync(id);

            ViewBag.CategoryId = id;

            if (products.Any())
            {
                ViewBag.CategoryName = products.First().CategoryName;
            }
            else
            {
                ViewBag.CategoryName = "Kategori";
            }

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? search, string? categoryId)
        {
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

            return View(product);
        }
    }
}