using System.Text.Json;
using FoodMart.Dtos.CartDtos;
using FoodMart.Services.ProductServices;
using MongoDB.Bson;

namespace FoodMart.Services.CartServices;

public class CartService(IHttpContextAccessor context, IProductService products)
{
    private const string SessionKey = "FoodMart.Cart";
    private const int MaxItems = 50;
    private const int MaxQuantity = 99;
    private CartDto? _cart;
    private ISession Session => context.HttpContext!.Session;

    private async Task<Dictionary<string, int>> ReadAsync()
    {
        await Session.LoadAsync(context.HttpContext!.RequestAborted);
        var json = Session.GetString(SessionKey);
        return json is null ? [] : JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? [];
    }

    private void Save(Dictionary<string, int> items)
    {
        Session.SetString(SessionKey, JsonSerializer.Serialize(items));
        _cart = null;
    }

    public async Task<CartDto> GetAsync()
    {
        if (_cart is not null) return _cart;
        var stored = await ReadAsync();
        var valid = new Dictionary<string, int>();
        var cart = new CartDto();
        foreach (var (id, quantity) in stored.Take(MaxItems))
        {
            if (!ObjectId.TryParse(id, out _) || quantity <= 0) continue;
            var product = await products.GetByIdAsync(id);
            if (product is null || !product.IsActive || product.Stock <= 0) continue;
            var available = Math.Min(quantity, Math.Min(product.Stock, MaxQuantity));
            valid[id] = available;
            cart.Items.Add(new CartItemDto
            {
                ProductId = id, Name = product.Name, ImageUrl = product.ImageUrl,
                Quantity = available, Stock = Math.Min(product.Stock, MaxQuantity),
                UnitPrice = product.DiscountPrice is > 0 && product.DiscountPrice < product.Price
                    ? product.DiscountPrice.Value : product.Price
            });
        }
        if (stored.Count != valid.Count || stored.Any(x => !valid.TryGetValue(x.Key, out var quantity) || quantity != x.Value)) Save(valid);
        return _cart = cart;
    }

    public async Task<string?> SetQuantityAsync(string id, int quantity, bool add)
    {
        if (!ObjectId.TryParse(id, out _) || quantity is < 1 or > MaxQuantity)
            return "Lütfen 1 ile 99 arasında bir adet giriniz.";
        var cart = await GetAsync();
        var stored = cart.Items.ToDictionary(x => x.ProductId, x => x.Quantity);
        var product = await products.GetByIdAsync(id);
        if (product is null || !product.IsActive || product.Stock <= 0) return "Bu ürün şu anda satışa uygun değil.";
        if (!stored.ContainsKey(id) && stored.Count >= MaxItems) return "Sepetinize en fazla 50 farklı ürün ekleyebilirsiniz.";
        if (!add && !stored.ContainsKey(id)) return "Ürün sepetinizde bulunamadı.";
        var totalQuantity = add ? stored.GetValueOrDefault(id) + quantity : quantity;
        if (totalQuantity > Math.Min(product.Stock, MaxQuantity)) return $"Bu üründen en fazla {Math.Min(product.Stock, MaxQuantity)} adet ekleyebilirsiniz.";
        stored[id] = totalQuantity;
        Save(stored);
        return null;
    }

    public async Task RemoveAsync(string id)
    {
        var stored = await ReadAsync();
        stored.Remove(id);
        Save(stored);
    }

    public async Task ClearAsync()
    {
        await Session.LoadAsync(context.HttpContext!.RequestAborted);
        Session.Remove(SessionKey);
        _cart = null;
    }
}
