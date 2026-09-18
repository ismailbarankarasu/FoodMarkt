using FoodMart.Dtos.DashboardDtos;
using FoodMart.Dtos.SaleDtos;
using FoodMart.Entities;
using MongoDB.Driver;

namespace FoodMart.Services.DashboardServices
{
    public class DashboardService : IDashboardService
    {
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IMongoCollection<Sale> _saleCollection;
        private readonly IMongoCollection<Subscriber> _subscriberCollection;

        public DashboardService(IMongoDatabase database)
        {
            _productCollection = database.GetCollection<Product>("Products");

            _categoryCollection = database.GetCollection<Category>("Categories");

            _saleCollection = database.GetCollection<Sale>("Sales");

            _subscriberCollection = database.GetCollection<Subscriber>("Subscribers");
        }

        public async Task<DashboardDto> GetDashboardAsync()
        {
            var now = DateTime.UtcNow;

            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var sevenDaysAgo = now.Date.AddDays(-6);

            var products = await _productCollection
                .Find(_ => true)
                .ToListAsync();

            var categories = await _categoryCollection
                .Find(_ => true)
                .ToListAsync();

            var sales = await _saleCollection
                .Find(_ => true)
                .SortByDescending(x => x.SaleDate)
                .ToListAsync();

            var subscribers = await _subscriberCollection
                .Find(_ => true)
                .ToListAsync();


            var productDictionary = products
                .ToDictionary(x => x.Id);

            var categoryDictionary = categories
                .ToDictionary(x => x.Id);


            var totalSalesQuantity =
                sales.Sum(x => x.Quantity);

            var totalRevenue =
                sales.Sum(x => x.TotalPrice);


            var recentSales = sales
                .Take(5)
                .Select(x => new DashboardSaleDto
                {
                    Id = x.Id,

                    ProductName =
                        productDictionary.TryGetValue(
                            x.ProductId,
                            out var product)
                            ? product.Name
                            : "Ürün Bulunamadı",

                    Quantity = x.Quantity,
                    TotalPrice = x.TotalPrice,
                    SaleDate = x.SaleDate
                })
                .ToList();


            var popularProducts = sales
                .GroupBy(x => x.ProductId)
                .Select(group => new
                {
                    ProductId = group.Key,
                    TotalQuantitySold =
                        group.Sum(x => x.Quantity),

                    TotalRevenue =
                        group.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x =>
                    x.TotalQuantitySold)
                .Take(4)
                .Select(x =>
                {
                    if (!productDictionary.TryGetValue(
                            x.ProductId,
                            out var product))
                    {
                        return null;
                    }

                    var categoryName =
                        categoryDictionary.TryGetValue(
                            product.CategoryId,
                            out var category)
                            ? category.Name
                            : "Kategori Bulunamadı";

                    return new PopularProductDto
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        CategoryName = categoryName,
                        ImageUrl = product.ImageUrl,
                        Price = product.Price,
                        DiscountPrice =
                            product.DiscountPrice,
                        Stock = product.Stock,
                        TotalQuantitySold =
                            x.TotalQuantitySold,
                        TotalRevenue =
                            x.TotalRevenue
                    };
                })
                .Where(x => x is not null)
                .Select(x => x!)
                .ToList();


            var categorySales = sales
                .Where(x =>
                    productDictionary.ContainsKey(
                        x.ProductId))
                .GroupBy(x =>
                    productDictionary[x.ProductId]
                        .CategoryId)
                .Select(group =>
                {
                    var categoryName =
                        categoryDictionary.TryGetValue(
                            group.Key,
                            out var category)
                            ? category.Name
                            : "Kategori Bulunamadı";

                    return new DashboardCategoryDto
                    {
                        CategoryName =
                            categoryName,

                        TotalQuantitySold =
                            group.Sum(x => x.Quantity)
                    };
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .ToList();


            var last7DaysSales =
                Enumerable.Range(0, 7)
                    .Select(index =>
                    {
                        var date =
                            sevenDaysAgo.AddDays(index);

                        var daySales = sales
                            .Where(x =>
                                x.SaleDate.Date ==
                                date.Date)
                            .ToList();

                        return new DashboardDailySaleDto
                        {
                            Date = date,

                            TotalQuantitySold =
                                daySales.Sum(x =>
                                    x.Quantity),

                            TotalRevenue =
                                daySales.Sum(x =>
                                    x.TotalPrice)
                        };
                    })
                    .ToList();


            return new DashboardDto
            {
                TotalProducts = products.Count,

                TotalCategories = categories.Count,

                TotalSalesQuantity = totalSalesQuantity,

                TotalRevenue = totalRevenue,

                TotalSubscribers = subscribers.Count,

                NewProductsThisMonth = products.Count(x => x.CreatedAt >= startOfMonth),

                SalesThisMonth =
                    sales
                        .Where(x => x.SaleDate >= startOfMonth)
                        .Sum(x => x.Quantity),

                RevenueThisMonth =
                    sales
                        .Where(x => x.SaleDate >= startOfMonth)
                        .Sum(x => x.TotalPrice),

                NewSubscribersLast7Days =
                    subscribers.Count(x => x.CreatedAt >= sevenDaysAgo),

                Last7DaysSales = last7DaysSales,

                CategorySales = categorySales,

                RecentSales = recentSales,

                PopularProducts = popularProducts
            };
        }
    }
}