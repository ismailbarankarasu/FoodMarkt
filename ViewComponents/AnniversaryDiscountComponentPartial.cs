using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class AnniversaryDiscountComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}