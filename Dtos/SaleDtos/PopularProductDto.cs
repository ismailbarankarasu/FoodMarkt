namespace FoodMart.Dtos.SaleDtos
{
    public class PopularProductDto
    {
        public string ProductId { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;

        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }

        public int Stock { get; set; }

        public int TotalQuantitySold { get; set; }

        public decimal TotalRevenue { get; set; }
    }
}