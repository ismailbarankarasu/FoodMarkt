using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class _LayoutHeaderComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
