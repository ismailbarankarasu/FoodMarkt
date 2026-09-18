using FoodMart.Dtos.DiscountDtos;
using FoodMart.Entities;
using MongoDB.Driver;

namespace FoodMart.Services.DiscountServices
{
    public class DiscountService : IDiscountService
    {
        private readonly IMongoCollection<Discount> _discountCollection;
        private readonly IMongoCollection<Product> _productCollection;

        public DiscountService(IMongoDatabase database)
        {
            _discountCollection = database.GetCollection<Discount>("Discounts");

            _productCollection = database.GetCollection<Product>("Products");
        }

        public async Task<List<ResultDiscountDto>> GetAllAsync()
        {
            var discounts = await _discountCollection
                .Find(_ => true)
                .SortByDescending(x => x.StartDate)
                .ToListAsync();

            return await MapDiscountsAsync(discounts);
        }

        public async Task<List<ResultDiscountDto>> GetActiveDiscountsAsync(int count)
        {
            if (count <= 0)
                return [];

            var now = DateTime.UtcNow;

            var discounts = await _discountCollection
                .Find(x =>
                    x.IsActive &&
                    x.StartDate <= now &&
                    x.EndDate >= now)
                .SortByDescending(x => x.DiscountRate)
                .Limit(count)
                .ToListAsync();

            return await MapDiscountsAsync(discounts);
        }

        public async Task<GetByIdDiscountDto?> GetByIdAsync(string id)
        {
            var discount = await _discountCollection
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (discount is null)
                return null;

            return new GetByIdDiscountDto
            {
                Id = discount.Id,
                Title = discount.Title,
                Description = discount.Description,
                DiscountRate = discount.DiscountRate,
                ImageUrl = discount.ImageUrl,
                ProductId = discount.ProductId,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                IsActive = discount.IsActive
            };
        }

        public async Task CreateAsync(
            CreateDiscountDto dto)
        {
            var discount = new Discount
            {
                Title = dto.Title,
                Description = dto.Description,
                DiscountRate = dto.DiscountRate,
                ImageUrl = dto.ImageUrl,
                ProductId = dto.ProductId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive
            };

            await _discountCollection.InsertOneAsync(discount);
        }

        public async Task UpdateAsync(
            UpdateDiscountDto dto)
        {
            var discount = new Discount
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                DiscountRate = dto.DiscountRate,
                ImageUrl = dto.ImageUrl,
                ProductId = dto.ProductId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive
            };

            await _discountCollection.ReplaceOneAsync(x => x.Id == dto.Id, discount);
        }

        public async Task DeleteAsync(string id)
        {
            await _discountCollection.DeleteOneAsync(x => x.Id == id);
        }

        private async Task<List<ResultDiscountDto>> MapDiscountsAsync(List<Discount> discounts)
        {
            if (discounts.Count == 0)
                return [];

            var productIds = discounts
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();

            var products = await _productCollection
                .Find(x => productIds.Contains(x.Id))
                .ToListAsync();

            var productDictionary = products
                .ToDictionary(x => x.Id, x => x.Name);

            return discounts.Select(x => new ResultDiscountDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                DiscountRate = x.DiscountRate,
                ImageUrl = x.ImageUrl,
                ProductId = x.ProductId,

                ProductName = productDictionary
                    .GetValueOrDefault(x.ProductId, "Ürün Bulunamadı"),

                StartDate = x.StartDate,
                EndDate = x.EndDate,
                IsActive = x.IsActive

            }).ToList();
        }
    }
}