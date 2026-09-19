namespace FoodMart.Dtos.ProductDtos
{
    public class CreateProductDto
    {
        public string CategoryId { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }

        public int Stock { get; set; }

        public IFormFile? ImageFile { get; set; }

        public string ImageUrl { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; }
    }
}