using FoodMart.Services.ImageServices;
using FoodMart.Dtos.DiscountDtos;
using FoodMart.Services.DiscountServices;
using FoodMart.Services.ProductServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FoodMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DiscountController : Controller
    {
        private readonly ImageService _imageService;
        private readonly IDiscountService _discountService;
        private readonly IProductService _productService;

        public DiscountController(
            ImageService imageService, IDiscountService discountService,
            IProductService productService)
        {
            _imageService = imageService;
            _discountService = discountService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var discounts = await _discountService.GetAllAsync();
            return View(discounts);
        }

        [HttpGet]
        public async Task<IActionResult> CreateDiscount()
        {
            await LoadProductsAsync();

            return View(new CreateDiscountDto
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7),
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDiscount(
            CreateDiscountDto createDiscountDto)
        {
            if (createDiscountDto.ImageFile is not null)
            {
                var error = await _imageService.ValidateAsync(createDiscountDto.ImageFile, HttpContext.RequestAborted);
                if (error is not null) ModelState.AddModelError("ImageFile", error);
            }

            if (!ModelState.IsValid)
            {
                await LoadProductsAsync();
                return View(createDiscountDto);
            }

            var uploadedUrl = createDiscountDto.ImageFile is null ? null
                : await _imageService.SaveAsync(createDiscountDto.ImageFile, HttpContext.RequestAborted);
            if (uploadedUrl is not null) createDiscountDto.ImageUrl = uploadedUrl;
            try
            {
                await _discountService.CreateAsync(createDiscountDto);
            }
            catch
            {
                await _imageService.DeleteIfUnusedAsync(uploadedUrl);
                throw;
            }


            TempData["AdminSuccess"] = "İşlem başarıyla tamamlandı.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateDiscount(string id)
        {
            var discount = await _discountService.GetByIdAsync(id);

            if (discount is null)
                return NotFound();

            await LoadProductsAsync();

            var dto = new UpdateDiscountDto
            {
                Id = discount.Id,
                Title = discount.Title,
                Description = discount.Description,
                DiscountRate = discount.DiscountRate,
                ImageUrl = discount.ImageUrl,
                ProductId = discount.ProductId,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                IsActive = discount.IsActive
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDiscount(
            UpdateDiscountDto updateDiscountDto)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(updateDiscountDto.Id, out _)) return NotFound();
            var existing = await _discountService.GetByIdAsync(updateDiscountDto.Id);
            if (existing is null) return NotFound();
            if (string.IsNullOrWhiteSpace(updateDiscountDto.ImageUrl)) updateDiscountDto.ImageUrl = existing.ImageUrl;

            if (updateDiscountDto.ImageFile is not null)
            {
                var error = await _imageService.ValidateAsync(updateDiscountDto.ImageFile, HttpContext.RequestAborted);
                if (error is not null) ModelState.AddModelError("ImageFile", error);
            }

            if (!ModelState.IsValid)
            {
                await LoadProductsAsync();
                return View(updateDiscountDto);
            }

            var uploadedUrl = updateDiscountDto.ImageFile is null ? null
                : await _imageService.SaveAsync(updateDiscountDto.ImageFile, HttpContext.RequestAborted);
            if (uploadedUrl is not null) updateDiscountDto.ImageUrl = uploadedUrl;
            try
            {
                await _discountService.UpdateAsync(updateDiscountDto);
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
        public async Task<IActionResult> DeleteDiscount(string id)
        {
            var existing = await _discountService.GetByIdAsync(id);
            if (existing is null) return NotFound();
            await _discountService.DeleteAsync(id);
            await _imageService.DeleteIfUnusedAsync(existing.ImageUrl);

            TempData["AdminSuccess"] = "İşlem başarıyla tamamlandı.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadProductsAsync()
        {
            var products = await _productService.GetAllAsync();

            ViewBag.Products = new SelectList(
                products,
                "Id",
                "Name");
        }
    }
}