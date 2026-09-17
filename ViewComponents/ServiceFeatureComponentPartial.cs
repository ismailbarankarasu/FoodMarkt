using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class ServiceFeatureComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}