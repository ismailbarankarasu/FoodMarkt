using FoodMart.Services.ImageServices;
using FoodMart.Dtos.ProductDtos;
using FoodMart.Services.CategoryServices;
using FoodMart.Services.ProductServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FoodMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ImageService _imageService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(ImageService imageService, IProductService productService, ICategoryService categoryService)
        {
            _imageService = imageService;
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            await LoadCategoriesAsync();

            return View(new CreateProductDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(CreateProductDto createProductDto)
        {
            if (createProductDto.ImageFile is not null)
            {
                var error = await _imageService.ValidateAsync(createProductDto.ImageFile, HttpContext.RequestAborted);
                if (error is not null) ModelState.AddModelError("ImageFile", error);
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return View(createProductDto);
            }

            var uploadedUrl = createProductDto.ImageFile is null ? null
                : await _imageService.SaveAsync(createProductDto.ImageFile, HttpContext.RequestAborted);
            if (uploadedUrl is not null) createProductDto.ImageUrl = uploadedUrl;
            try
            {
                await _productService.CreateAsync(createProductDto);
            }
            catch
            {
                await _imageService.DeleteIfUnusedAsync(uploadedUrl);
                throw;
            }


            TempData["AdminSuccess"] = "İşlem başarıyla tamamlandı.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _categoryService.GetAllAsync();

            ViewBag.Categories = new SelectList(
                categories,
                "Id",
                "Name");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProduct(string id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product is null)
                return NotFound();

            await LoadCategoriesAsync();

            var updateProductDto = new UpdateProductDto
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                IsFeatured = product.IsFeatured
            };

            return View(updateProductDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProduct(UpdateProductDto updateProductDto)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(updateProductDto.Id, out _)) return NotFound();
            var existing = await _productService.GetByIdAsync(updateProductDto.Id);
            if (existing is null) return NotFound();
            if (string.IsNullOrWhiteSpace(updateProductDto.ImageUrl)) updateProductDto.ImageUrl = existing.ImageUrl;

            if (updateProductDto.ImageFile is not null)
            {
                var error = await _imageService.ValidateAsync(updateProductDto.ImageFile, HttpContext.RequestAborted);
                if (error is not null) ModelState.AddModelError("ImageFile", error);
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return View(updateProductDto);
            }

            var uploadedUrl = updateProductDto.ImageFile is null ? null
                : await _imageService.SaveAsync(updateProductDto.ImageFile, HttpContext.RequestAborted);
            if (uploadedUrl is not null) updateProductDto.ImageUrl = uploadedUrl;
            try
            {
                await _productService.UpdateAsync(updateProductDto);
            }
            catch
            {
                await _imageService.DeleteIfUnusedAsync(uploadedUrl);
                throw;
            }
            await _imageService.DeleteIfUnusedAsync(existing.ImageUrl);


            TempData["AdminSuccess"] = "İşlem başarıyla tamamlandı.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var existing = await _productService.GetByIdAsync(id);
            if (existing is null) return NotFound();
            await _productService.DeleteAsync(id);
            await _imageService.DeleteIfUnusedAsync(existing.ImageUrl);

            TempData["AdminSuccess"] = "İşlem başarıyla tamamlandı.";
            return RedirectToAction(nameof(Index));
        }
    }
}