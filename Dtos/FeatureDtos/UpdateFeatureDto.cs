namespace FoodMart.Dtos.FeatureDtos
{
    public class UpdateFeatureDto
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public IFormFile? ImageFile { get; set; }

        public string ImageUrl { get; set; } = null!;
        public string ButtonText { get; set; } = null!;
        public string ButtonUrl { get; set; } = null!;
        public int Order { get; set; }
        public bool IsActive { get; set; }
    }
}