using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class PopularProductsComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}