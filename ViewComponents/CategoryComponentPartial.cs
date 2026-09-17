using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class CategoryComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}