using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class FeatureComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}