using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class _LayoutLogoComponentPartial: ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
