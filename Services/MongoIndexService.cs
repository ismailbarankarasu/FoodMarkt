using FoodMart.Entities;
using MongoDB.Driver;

namespace FoodMart.Services;

public sealed class MongoIndexService(IMongoDatabase database)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Match actual filters and sorts. Unanchored search cannot use a normal Name index.
        await Add<Product>("Products", Builders<Product>.IndexKeys.Descending(x => x.CreatedAt), "created", cancellationToken);
        await Add<Product>("Products", Builders<Product>.IndexKeys.Ascending(x => x.IsActive).Descending(x => x.CreatedAt), "active_created", cancellationToken);
        await Add<Product>("Products", Builders<Product>.IndexKeys.Ascending(x => x.CategoryId).Ascending(x => x.IsActive).Descending(x => x.CreatedAt), "category_active_created", cancellationToken);
        await Add<Feature>("Features", Builders<Feature>.IndexKeys.Ascending(x => x.Order), "order", cancellationToken);
        await Add<Feature>("Features", Builders<Feature>.IndexKeys.Ascending(x => x.IsActive).Ascending(x => x.Order), "active_order", cancellationToken);
        await Add<Discount>("Discounts", Builders<Discount>.IndexKeys.Descending(x => x.StartDate), "start", cancellationToken);
        await Add<Discount>("Discounts", Builders<Discount>.IndexKeys.Ascending(x => x.IsActive).Descending(x => x.DiscountRate).Ascending(x => x.EndDate).Ascending(x => x.StartDate), "active_rate_dates", cancellationToken);
        await Add<Sale>("Sales", Builders<Sale>.IndexKeys.Descending(x => x.SaleDate), "sale_date", cancellationToken);
        await Add<Subscriber>("Subscribers", Builders<Subscriber>.IndexKeys.Descending(x => x.CreatedAt), "created", cancellationToken);
        await Add<Subscriber>("Subscribers", Builders<Subscriber>.IndexKeys.Ascending(x => x.Email), "email_unique", cancellationToken, true);
        await Add<Subscriber>("Subscribers", Builders<Subscriber>.IndexKeys.Ascending(x => x.DiscountCode), "discount_code_unique", cancellationToken, true);
        await Add<AdminUser>("AdminUsers", Builders<AdminUser>.IndexKeys.Ascending(x => x.Email), "email_unique", cancellationToken, true);
        // Categories are read in full or by their automatically indexed _id.
    }

    private Task<string> Add<T>(string collection, IndexKeysDefinition<T> keys, string name, CancellationToken token, bool unique = false) =>
        database.GetCollection<T>(collection).Indexes.CreateOneAsync(
            new CreateIndexModel<T>(keys, new CreateIndexOptions { Name = name, Unique = unique }), cancellationToken: token);
}
