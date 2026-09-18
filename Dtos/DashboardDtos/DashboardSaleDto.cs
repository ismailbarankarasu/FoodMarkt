namespace FoodMart.Dtos.DashboardDtos
{
    public class DashboardSaleDto
    {
        public string Id { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime SaleDate { get; set; }
    }
}