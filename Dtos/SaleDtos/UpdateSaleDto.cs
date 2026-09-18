namespace FoodMart.Dtos.SaleDtos
{
    public class UpdateSaleDto
    {
        public string Id { get; set; } = null!;
        public string ProductId { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime SaleDate { get; set; }
    }
}