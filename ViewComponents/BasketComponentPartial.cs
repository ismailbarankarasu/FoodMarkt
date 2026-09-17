using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class BasketComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
