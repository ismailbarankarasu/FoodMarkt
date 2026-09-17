using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class DiscountComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}