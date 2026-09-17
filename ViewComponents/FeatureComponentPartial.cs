using FoodMart.Services.FeatureServices;
using Microsoft.AspNetCore.Mvc;

namespace FoodMart.ViewComponents
{
    public class FeatureComponentPartial : ViewComponent
    {
        private readonly IFeatureService _featureService;

        public FeatureComponentPartial(IFeatureService featureService)
        {
            _featureService = featureService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var features = await _featureService.GetActiveFeaturesAsync();

            return View(features);
        }
    }
}