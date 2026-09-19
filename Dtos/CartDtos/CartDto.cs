namespace FoodMart.Dtos.CartDtos;

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = [];
    public int TotalQuantity => Items.Sum(x => x.Quantity);
    public decimal TotalPrice => Items.Sum(x => x.TotalPrice);
}

public class CartItemDto
{
    public string ProductId { get; set; } = "";
    public string Name { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public int Quantity { get; set; }
    public int Stock { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}
