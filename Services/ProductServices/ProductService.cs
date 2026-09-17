using FoodMart.Dtos.ProductDtos;
using FoodMart.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace FoodMart.Services.ProductServices
{
    public class ProductService : IProductService
    {
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<Category> _categoryCollection;

        public ProductService(IMongoDatabase database)
        {
            _productCollection = database.GetCollection<Product>("Products");

            _categoryCollection = database.GetCollection<Category>("Categories");
        }

        public async Task<List<ResultProductDto>> GetAllAsync()
        {
            var products = await _productCollection
                .Find(_ => true)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();

            return await MapProductsAsync(products);
        }

        public async Task<GetByIdProductDto?> GetByIdAsync(string id)
        {
            var product = await _productCollection
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (product is null)
                return null;

            return new GetByIdProductDto
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                CreatedAt = product.CreatedAt,
                IsActive = product.IsActive,
                IsFeatured = product.IsFeatured
            };
        }

        public async Task CreateAsync(CreateProductDto createProductDto)
        {
            var product = new Product
            {
                CategoryId = createProductDto.CategoryId,
                Name = createProductDto.Name,
                Description = createProductDto.Description,
                Price = createProductDto.Price,
                DiscountPrice = createProductDto.DiscountPrice,
                Stock = createProductDto.Stock,
                ImageUrl = createProductDto.ImageUrl,
                CreatedAt = DateTime.UtcNow,
                IsActive = createProductDto.IsActive,
                IsFeatured = createProductDto.IsFeatured
            };

            await _productCollection.InsertOneAsync(product);
        }

        public async Task UpdateAsync(UpdateProductDto updateProductDto)
        {
            var currentProduct = await _productCollection
                .Find(x => x.Id == updateProductDto.Id)
                .FirstOrDefaultAsync();

            if (currentProduct is null)
                return;

            var product = new Product
            {
                Id = updateProductDto.Id,
                CategoryId = updateProductDto.CategoryId,
                Name = updateProductDto.Name,
                Description = updateProductDto.Description,
                Price = updateProductDto.Price,
                DiscountPrice = updateProductDto.DiscountPrice,
                Stock = updateProductDto.Stock,
                ImageUrl = updateProductDto.ImageUrl,

                // Güncellemede ilk oluşturulma tarihini koruyoruz.
                CreatedAt = currentProduct.CreatedAt,

                IsActive = updateProductDto.IsActive,
                IsFeatured = updateProductDto.IsFeatured
            };

            await _productCollection.ReplaceOneAsync(
                x => x.Id == updateProductDto.Id,
                product);
        }

        public async Task DeleteAsync(string id)
        {
            await _productCollection.DeleteOneAsync(
                x => x.Id == id);
        }

        public async Task<List<ResultProductDto>>
            GetLatestProductsAsync(int count)
        {
            if (count <= 0)
                return new List<ResultProductDto>();

            var products = await _productCollection
                .Find(x => x.IsActive)
                .SortByDescending(x => x.CreatedAt)
                .Limit(count)
                .ToListAsync();

            return await MapProductsAsync(products);
        }

        public async Task<List<ResultProductDto>> GetProductsByCategoryAsync(string categoryId)
        {
            var products = await _productCollection
                .Find(x =>
                    x.CategoryId == categoryId &&
                    x.IsActive)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();

            return await MapProductsAsync(products);
        }

        public async Task<List<ResultProductDto>> SearchProductsAsync(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return new List<ResultProductDto>();

            search = search.Trim();

            var escapedSearch = Regex.Escape(search);

            var regex = new BsonRegularExpression(
                escapedSearch,
                "i");

            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Eq(
                    x => x.IsActive,
                    true),

                Builders<Product>.Filter.Or(
                    Builders<Product>.Filter.Regex(
                        x => x.Name,
                        regex),

                    Builders<Product>.Filter.Regex(
                        x => x.Description,
                        regex)
                )
            );

            var products = await _productCollection
                .Find(filter)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();

            return await MapProductsAsync(products);
        }

        private async Task<List<ResultProductDto>> MapProductsAsync(List<Product> products)
        {
            if (products.Count == 0)
                return new List<ResultProductDto>();

            var categoryIds = products
                .Select(x => x.CategoryId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var categories = await _categoryCollection
                .Find(x => categoryIds.Contains(x.Id))
                .ToListAsync();

            var categoryDictionary = categories
                .ToDictionary(
                    x => x.Id,
                    x => x.Name);

            return products.Select(product =>
            {
                categoryDictionary.TryGetValue(
                    product.CategoryId,
                    out var categoryName);

                return new ResultProductDto
                {
                    Id = product.Id,
                    CategoryId = product.CategoryId,
                    CategoryName =
                        categoryName ?? "Kategori Bulunamadı",

                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    DiscountPrice = product.DiscountPrice,
                    Stock = product.Stock,
                    ImageUrl = product.ImageUrl,
                    CreatedAt = product.CreatedAt,
                    IsActive = product.IsActive,
                    IsFeatured = product.IsFeatured
                };
            }).ToList();
        }
    }
}