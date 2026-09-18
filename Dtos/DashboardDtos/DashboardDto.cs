using FoodMart.Dtos.SaleDtos;

namespace FoodMart.Dtos.DashboardDtos
{
    public class DashboardDto
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }

        public int TotalSalesQuantity { get; set; }
        public decimal TotalRevenue { get; set; }

        public int TotalSubscribers { get; set; }

        public int NewProductsThisMonth { get; set; }
        public int SalesThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int NewSubscribersLast7Days { get; set; }

        public List<DashboardDailySaleDto> Last7DaysSales { get; set; } = [];

        public List<DashboardCategoryDto> CategorySales { get; set; } = [];

        public List<DashboardSaleDto> RecentSales { get; set; } = [];

        public List<PopularProductDto> PopularProducts { get; set; } = [];
    }
}