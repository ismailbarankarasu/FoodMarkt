namespace FoodMart.Dtos.DashboardDtos
{
    public class DashboardDailySaleDto
    {
        public DateTime Date { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}