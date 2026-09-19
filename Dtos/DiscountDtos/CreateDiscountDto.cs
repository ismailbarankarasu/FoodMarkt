namespace FoodMart.Dtos.DiscountDtos
{
    public class CreateDiscountDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int DiscountRate { get; set; }
        public IFormFile? ImageFile { get; set; }

        public string ImageUrl { get; set; } = null!;
        public string ProductId { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}