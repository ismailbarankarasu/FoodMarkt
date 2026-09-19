using FoodMart.Services.CartServices;
using Microsoft.AspNetCore.Mvc;

namespace FoodMart.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class CartController(CartService cart) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index() => View(await cart.GetAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string id, int quantity = 1, string? returnUrl = null)
    {
        var error = ModelState.IsValid ? await cart.SetQuantityAsync(id, quantity, true) : "Lütfen geçerli bir adet giriniz.";
        TempData[error is null ? "CartSuccess" : "CartError"] = error ?? "Ürün sepetinize eklendi.";
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string id, int quantity)
    {
        var error = ModelState.IsValid ? await cart.SetQuantityAsync(id, quantity, false) : "Lütfen geçerli bir adet giriniz.";
        TempData[error is null ? "CartSuccess" : "CartError"] = error ?? "Sepetiniz güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(string id)
    {
        await cart.RemoveAsync(id);
        TempData["CartSuccess"] = "Ürün sepetten çıkarıldı.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        await cart.ClearAsync();
        TempData["CartSuccess"] = "Sepetiniz temizlendi.";
        return RedirectToAction(nameof(Index));
    }
}
