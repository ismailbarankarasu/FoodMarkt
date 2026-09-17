using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents.Admin
{
    public class _AdminHeaderComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}