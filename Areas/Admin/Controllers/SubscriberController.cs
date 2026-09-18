using FoodMart.Services.SubscriberServices;
using Microsoft.AspNetCore.Mvc;

namespace FoodMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubscriberController : Controller
    {
        private readonly ISubscriberService _subscriberService;

        public SubscriberController(ISubscriberService subscriberService)
        {
            _subscriberService = subscriberService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var subscribers = await _subscriberService.GetAllAsync();

            return View(subscribers);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var subscriber = await _subscriberService.GetByIdAsync(id);

            if (subscriber is null)
                return NotFound();

            return View(subscriber);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubscriber(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            await _subscriberService.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}