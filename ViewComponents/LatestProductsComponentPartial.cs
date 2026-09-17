using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class LatestProductsComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}