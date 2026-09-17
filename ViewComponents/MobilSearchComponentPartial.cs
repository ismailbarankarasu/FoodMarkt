using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class MobilSearchComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
