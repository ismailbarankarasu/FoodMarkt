using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class _LayoutHeadComponentPartial: ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
