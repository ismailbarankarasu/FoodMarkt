using FoodMart.Dtos.SubscriberDtos;
using FoodMart.Services.EmailServices;
using FoodMart.Services.SubscriberServices;
using Microsoft.AspNetCore.Mvc;


namespace FoodMart.Controllers
{
    public class SubscribeController : Controller
    {
        private readonly ISubscriberService _subscriberService;
        private readonly IEmailService _emailService;
        private readonly ILogger<SubscribeController> _logger;

        public SubscribeController(ISubscriberService subscriberService, IEmailService emailService, ILogger<SubscribeController> logger)
        {
            _subscriberService = subscriberService;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSubscriberDto createSubscriberDto)
        {
            if (!ModelState.IsValid)
            {
                TempData["SubscribeError"] = string.Join(" ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return RedirectToHome();
            }

            var emailExists = await _subscriberService.EmailExistsAsync(createSubscriberDto.Email);

            if (emailExists)
            {
                TempData["SubscribeError"] = "Bu e-posta adresi kampanyaya daha önce katılmış.";

                return RedirectToHome();
            }

            ResultSubscriberDto? subscriber = null;

            try
            {
                subscriber = await _subscriberService.CreateAsync(createSubscriberDto);

                await _emailService.SendDiscountCodeAsync(
                    subscriber.FullName,
                    subscriber.Email,
                    subscriber.DiscountCode,
                    subscriber.DiscountRate,
                    subscriber.ExpiresAt);

                TempData["SubscribeSuccess"] = $"Tebrikler! %{subscriber.DiscountRate} indirim kodunuz oluşturuldu.";

                TempData["DiscountCode"] = subscriber.DiscountCode;

                return RedirectToHome();
            }

            catch (MongoDB.Driver.MongoWriteException ex) when (ex.WriteError.Category == MongoDB.Driver.ServerErrorCategory.DuplicateKey)
            {
                TempData["SubscribeError"] = "Bu e-posta adresi kampanyaya daha önce katılmış.";
                return RedirectToHome();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscriber discount email could not be sent.");
                if (subscriber is not null)
                {
                    try { await _subscriberService.DeleteAsync(subscriber.Id); }
                    catch (Exception cleanupError)
                    {
                        _logger.LogError(cleanupError, "Failed subscription cleanup could not be completed.");
                    }
                }

                TempData["SubscribeError"] = "İndirim kodunuz gönderilirken bir sorun oluştu. Lütfen tekrar deneyiniz.";

                return RedirectToHome();
            }
        }

        private IActionResult RedirectToHome()
        {
            return Redirect(
                Url.Action(
                    "Index",
                    "Default",
                    null,
                    Request.Scheme) + "#subscribe");
        }
    }
}
