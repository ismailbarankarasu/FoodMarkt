using FoodMart.Services.CartServices;
using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents;

public class BasketComponentPartial(CartService cart) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync() => View(await cart.GetAsync());
}
