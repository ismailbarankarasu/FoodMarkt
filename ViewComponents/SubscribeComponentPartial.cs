using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class SubscribeComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
