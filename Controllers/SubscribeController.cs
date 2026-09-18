using FoodMart.Dtos.SubscriberDtos;
using FoodMart.Services.EmailServices;
using FoodMart.Services.SubscriberServices;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
            if (string.IsNullOrWhiteSpace(createSubscriberDto.FullName))
            {
                TempData["SubscribeError"] = "Lütfen adınızı ve soyadınızı giriniz.";

                return RedirectToHome();
            }

            if (string.IsNullOrWhiteSpace(createSubscriberDto.Email))
            {
                TempData["SubscribeError"] = "Lütfen e-posta adresinizi giriniz.";

                return RedirectToHome();
            }

            var emailValidator = new EmailAddressAttribute();

            if (!emailValidator.IsValid(createSubscriberDto.Email))
            {
                TempData["SubscribeError"] = "Lütfen geçerli bir e-posta adresi giriniz.";

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

            catch (Exception ex)
            {
                if (subscriber is not null)
                {
                    await _subscriberService.DeleteAsync(subscriber.Id);
                }

                _logger.LogError(ex, "Subscriber discount email could not be sent to {Email}.", createSubscriberDto.Email);

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