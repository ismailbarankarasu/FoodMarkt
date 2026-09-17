using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class _LayoutScriptsComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
