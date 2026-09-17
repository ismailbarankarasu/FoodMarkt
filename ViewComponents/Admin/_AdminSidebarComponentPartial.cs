using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents.Admin
{
    public class _AdminSidebarComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}