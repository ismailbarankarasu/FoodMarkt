using FoodMart.Services.DashboardServices;
using Microsoft.AspNetCore.Mvc;

namespace FoodMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var dashboard = await _dashboardService.GetDashboardAsync();

            return View(dashboard);
        }
    }
}