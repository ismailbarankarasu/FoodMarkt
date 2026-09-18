namespace FoodMart.Dtos.SaleDtos
{
    public class ResultSaleDto
    {
        public string Id { get; set; } = null!;
        public string ProductId { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime SaleDate { get; set; }
    }
}