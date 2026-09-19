using FoodMart.Services.ImageServices;
using FoodMart.Dtos.FeatureDtos;
using FoodMart.Services.FeatureServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FeatureController : Controller
    {
        private readonly ImageService _imageService;
        private readonly IFeatureService _featureService;

        public FeatureController(ImageService imageService, IFeatureService featureService)
        {
            _imageService = imageService;
            _featureService = featureService;
        }

        public async Task<IActionResult> Index()
        {
            var values = await _featureService.GetAllAsync();
            return View(values);
        }

        [HttpGet]
        public IActionResult CreateFeature()
        {
            return View(new CreateFeatureDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFeature(CreateFeatureDto createFeatureDto)
        {
            if (createFeatureDto.ImageFile is not null)
            {
                var error = await _imageService.ValidateAsync(createFeatureDto.ImageFile, HttpContext.RequestAborted);
                if (error is not null) ModelState.AddModelError("ImageFile", error);
            }

            if (!ModelState.IsValid)
                return View(createFeatureDto);

            var uploadedUrl = createFeatureDto.ImageFile is null ? null
                : await _imageService.SaveAsync(createFeatureDto.ImageFile, HttpContext.RequestAborted);
            if (uploadedUrl is not null) createFeatureDto.ImageUrl = uploadedUrl;
            try
            {
                await _featureService.CreateAsync(createFeatureDto);
            }
            catch
            {
                await _imageService.DeleteIfUnusedAsync(uploadedUrl);
                throw;
            }


            TempData["AdminSuccess"] = "İşlem başarıyla tamamlandı.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateFeature(string id)
        {
            var value = await _featureService.GetByIdAsync(id);

            if (value is null)
                return NotFound();

            var dto = new UpdateFeatureDto
            {
                Id = value.Id,
                Title = value.Title,
                Description = value.Description,
                ImageUrl = value.ImageUrl,
                ButtonText = value.ButtonText,
                ButtonUrl = value.ButtonUrl,
                Order = value.Order,
                IsActive = value.IsActive
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFeature(UpdateFeatureDto updateFeatureDto)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(updateFeatureDto.Id, out _)) return NotFound();
            var existing = await _featureService.GetByIdAsync(updateFeatureDto.Id);
            if (existing is null) return NotFound();
            if (string.IsNullOrWhiteSpace(updateFeatureDto.ImageUrl)) updateFeatureDto.ImageUrl = existing.ImageUrl;

            if (updateFeatureDto.ImageFile is not null)
            {
                var error = await _imageService.ValidateAsync(updateFeatureDto.ImageFile, HttpContext.RequestAborted);
                if (error is not null) ModelState.AddModelError("ImageFile", error);
            }

            if (!ModelState.IsValid)
                return View(updateFeatureDto);

            var uploadedUrl = updateFeatureDto.ImageFile is null ? null
                : await _imageService.SaveAsync(updateFeatureDto.ImageFile, HttpContext.RequestAborted);
            if (uploadedUrl is not null) updateFeatureDto.ImageUrl = uploadedUrl;
            try
            {
                await _featureService.UpdateAsync(updateFeatureDto);
            }
            catch
            {
                await _imageService.DeleteIfUnusedAsync(uploadedUrl);
                throw;
            }
            await _imageService.DeleteIfUnusedAsync(existing.ImageUrl);


            TempData["AdminSuccess"] = "İşlem başarıyla tamamlandı.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFeature(string id)
        {
            var existing = await _featureService.GetByIdAsync(id);
            if (existing is null) return NotFound();
            await _featureService.DeleteAsync(id);
            await _imageService.DeleteIfUnusedAsync(existing.ImageUrl);

            TempData["AdminSuccess"] = "İşlem başarıyla tamamlandı.";
            return RedirectToAction(nameof(Index));
        }
    }
}