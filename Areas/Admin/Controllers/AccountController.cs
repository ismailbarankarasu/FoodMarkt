using System.Security.Claims;
using FoodMart.Dtos.AdminDtos;
using FoodMart.Services.AdminAuthServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAdminAuthService _adminAuthService;

        public AccountController(IAdminAuthService adminAuthService, ILogger<AccountController> logger)
        {
            _adminAuthService = adminAuthService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard",
                    new { area = "Admin" });
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            AdminLoginDto adminLoginDto)
        {
            if (!ModelState.IsValid) return View(adminLoginDto);

            var admin = await _adminAuthService.ValidateUserAsync(adminLoginDto.Email, adminLoginDto.Password);

            if (admin is null)
            {
                _logger.LogWarning("Admin login failed.");
                ModelState.AddModelError(string.Empty, "E-posta adresi veya şifre hatalı.");
                return View(adminLoginDto);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, admin.Id),
                new(ClaimTypes.Name, admin.FullName),
                new(ClaimTypes.Email, admin.Email),
                new(ClaimTypes.Role, "Admin")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authenticationProperties = new AuthenticationProperties
                {
                    IsPersistent = adminLoginDto.RememberMe,
                    AllowRefresh = true
                };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authenticationProperties);
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login), "Account", new { area = "Admin" });
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}