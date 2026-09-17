using Microsoft.AspNetCore.Mvc;

namespace FoodMart.Controllers
{
    public class DefaultController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
