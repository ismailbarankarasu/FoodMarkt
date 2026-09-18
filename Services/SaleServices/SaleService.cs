using FoodMart.Dtos.SaleDtos;
using FoodMart.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FoodMart.Services.SaleServices
{
    public class SaleService : ISaleService
    {
        private readonly IMongoCollection<Sale> _saleCollection;
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<Category> _categoryCollection;

        public SaleService(IMongoDatabase database)
        {
            _saleCollection = database.GetCollection<Sale>("Sales");

            _productCollection = database.GetCollection<Product>("Products");

            _categoryCollection = database.GetCollection<Category>("Categories");
        }

        public async Task<List<ResultSaleDto>> GetAllAsync()
        {
            var sales = await _saleCollection
                .Find(_ => true)
                .SortByDescending(x => x.SaleDate)
                .ToListAsync();

            if (sales.Count == 0)
                return [];

            var productIds = sales
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();

            var products = await _productCollection
                .Find(x => productIds.Contains(x.Id))
                .ToListAsync();

            var productDictionary = products
                .ToDictionary(x => x.Id, x => x.Name);

            return sales.Select(x => new ResultSaleDto
            {
                Id = x.Id,
                ProductId = x.ProductId,

                ProductName = productDictionary.GetValueOrDefault(
                    x.ProductId,
                    "Ürün Bulunamadı"),

                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                TotalPrice = x.TotalPrice,
                SaleDate = x.SaleDate

            }).ToList();
        }

        public async Task<GetByIdSaleDto?> GetByIdAsync(string id)
        {
            var sale = await _saleCollection
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (sale is null)
                return null;

            return new GetByIdSaleDto
            {
                Id = sale.Id,
                ProductId = sale.ProductId,
                Quantity = sale.Quantity,
                UnitPrice = sale.UnitPrice,
                TotalPrice = sale.TotalPrice,
                SaleDate = sale.SaleDate
            };
        }

        public async Task CreateAsync(CreateSaleDto dto)
        {
            var sale = new Sale
            {
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,

                TotalPrice =
                    dto.Quantity * dto.UnitPrice,

                SaleDate = dto.SaleDate
            };

            await _saleCollection.InsertOneAsync(sale);
        }

        public async Task UpdateAsync(UpdateSaleDto dto)
        {
            var sale = new Sale
            {
                Id = dto.Id,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,

                TotalPrice =
                    dto.Quantity * dto.UnitPrice,

                SaleDate = dto.SaleDate
            };

            await _saleCollection.ReplaceOneAsync(
                x => x.Id == dto.Id,
                sale);
        }

        public async Task DeleteAsync(string id)
        {
            await _saleCollection.DeleteOneAsync(
                x => x.Id == id);
        }

        public async Task<List<PopularProductDto>> GetPopularProductsAsync(int count)
        {
            if (count <= 0)
                return [];

            var aggregation = await _saleCollection
                .Aggregate()
                .Group(
                    x => x.ProductId,
                    group => new
                    {
                        ProductId = group.Key,

                        TotalQuantitySold =
                            group.Sum(x => x.Quantity),

                        TotalRevenue =
                            group.Sum(x => x.TotalPrice)
                    })
                .SortByDescending(x => x.TotalQuantitySold)
                .Limit(count)
                .ToListAsync();

            if (aggregation.Count == 0)
                return [];

            var productIds = aggregation
                .Select(x => x.ProductId)
                .ToList();

            var products = await _productCollection
                .Find(x =>
                    productIds.Contains(x.Id) &&
                    x.IsActive)
                .ToListAsync();

            if (products.Count == 0)
                return [];

            var categoryIds = products
                .Select(x => x.CategoryId)
                .Distinct()
                .ToList();

            var categories = await _categoryCollection
                .Find(x => categoryIds.Contains(x.Id))
                .ToListAsync();

            var categoryDictionary = categories
                .ToDictionary(x => x.Id, x => x.Name);

            var productDictionary = products
                .ToDictionary(x => x.Id);

            var result = new List<PopularProductDto>();

            foreach (var saleInfo in aggregation)
            {
                if (!productDictionary.TryGetValue(
                        saleInfo.ProductId,
                        out var product))
                {
                    continue;
                }

                result.Add(new PopularProductDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,

                    CategoryName =
                        categoryDictionary.GetValueOrDefault(
                            product.CategoryId,
                            "Kategori Bulunamadı"),

                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    DiscountPrice = product.DiscountPrice,
                    Stock = product.Stock,

                    TotalQuantitySold =
                        saleInfo.TotalQuantitySold,

                    TotalRevenue =
                        saleInfo.TotalRevenue
                });
            }

            return result;
        }
    }
}