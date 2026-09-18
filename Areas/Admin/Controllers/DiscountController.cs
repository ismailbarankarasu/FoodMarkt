using FoodMart.Dtos.DiscountDtos;
using FoodMart.Services.DiscountServices;
using FoodMart.Services.ProductServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FoodMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DiscountController : Controller
    {
        private readonly IDiscountService _discountService;
        private readonly IProductService _productService;

        public DiscountController(
            IDiscountService discountService,
            IProductService productService)
        {
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
            if (createDiscountDto.EndDate < createDiscountDto.StartDate)
            {
                ModelState.AddModelError(
                    nameof(createDiscountDto.EndDate),
                    "Bitiş tarihi başlangıç tarihinden önce olamaz.");
            }

            if (createDiscountDto.DiscountRate <= 0 ||
                createDiscountDto.DiscountRate > 100)
            {
                ModelState.AddModelError(
                    nameof(createDiscountDto.DiscountRate),
                    "İndirim oranı 1 ile 100 arasında olmalıdır.");
            }

            if (!ModelState.IsValid)
            {
                await LoadProductsAsync();
                return View(createDiscountDto);
            }

            await _discountService.CreateAsync(createDiscountDto);

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
            if (updateDiscountDto.EndDate < updateDiscountDto.StartDate)
            {
                ModelState.AddModelError(
                    nameof(updateDiscountDto.EndDate),
                    "Bitiş tarihi başlangıç tarihinden önce olamaz.");
            }

            if (updateDiscountDto.DiscountRate <= 0 ||
                updateDiscountDto.DiscountRate > 100)
            {
                ModelState.AddModelError(
                    nameof(updateDiscountDto.DiscountRate),
                    "İndirim oranı 1 ile 100 arasında olmalıdır.");
            }

            if (!ModelState.IsValid)
            {
                await LoadProductsAsync();
                return View(updateDiscountDto);
            }

            await _discountService.UpdateAsync(updateDiscountDto);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDiscount(string id)
        {
            await _discountService.DeleteAsync(id);

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