using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class _LayoutFooterComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}