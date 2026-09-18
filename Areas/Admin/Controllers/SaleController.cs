using FoodMart.Dtos.SaleDtos;
using FoodMart.Services.ProductServices;
using FoodMart.Services.SaleServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FoodMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SaleController : Controller
    {
        private readonly ISaleService _saleService;
        private readonly IProductService _productService;

        public SaleController(ISaleService saleService, IProductService productService)
        {
            _saleService = saleService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var sales = await _saleService.GetAllAsync();
            return View(sales);
        }

        [HttpGet]
        public async Task<IActionResult> CreateSale()
        {
            await LoadProductsAsync();

            return View(new CreateSaleDto
            {
                Quantity = 1,
                SaleDate = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSale(CreateSaleDto createSaleDto)
        {
            ValidateSale(
                createSaleDto.ProductId,
                createSaleDto.Quantity,
                createSaleDto.UnitPrice);

            if (!ModelState.IsValid)
            {
                await LoadProductsAsync();
                return View(createSaleDto);
            }

            await _saleService.CreateAsync(createSaleDto);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateSale(string id)
        {
            var sale = await _saleService.GetByIdAsync(id);

            if (sale is null)
                return NotFound();

            await LoadProductsAsync();

            var dto = new UpdateSaleDto
            {
                Id = sale.Id,
                ProductId = sale.ProductId,
                Quantity = sale.Quantity,
                UnitPrice = sale.UnitPrice,
                SaleDate = sale.SaleDate
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSale(
            UpdateSaleDto updateSaleDto)
        {
            ValidateSale(
                updateSaleDto.ProductId,
                updateSaleDto.Quantity,
                updateSaleDto.UnitPrice);

            if (!ModelState.IsValid)
            {
                await LoadProductsAsync();
                return View(updateSaleDto);
            }

            await _saleService.UpdateAsync(updateSaleDto);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSale(string id)
        {
            await _saleService.DeleteAsync(id);

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

        private void ValidateSale(string? productId, int quantity, decimal unitPrice)
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                ModelState.AddModelError(
                    "ProductId",
                    "Lütfen bir ürün seçiniz.");
            }

            if (quantity <= 0)
            {
                ModelState.AddModelError(
                    "Quantity",
                    "Satış adedi 1 veya daha büyük olmalıdır.");
            }

            if (unitPrice <= 0)
            {
                ModelState.AddModelError(
                    "UnitPrice",
                    "Birim fiyat 0'dan büyük olmalıdır.");
            }
        }
    }
}