using FoodMart.Dtos.SubscriberDtos;
using FoodMart.Entities;
using MongoDB.Driver;

namespace FoodMart.Services.SubscriberServices
{
    public class SubscriberService : ISubscriberService
    {
        private readonly IMongoCollection<Subscriber> _subscriberCollection;

        public SubscriberService(IMongoDatabase database)
        {
            _subscriberCollection = database.GetCollection<Subscriber>("Subscribers");
        }


        public async Task<List<ResultSubscriberDto>> GetAllAsync()
        {
            var subscribers = await _subscriberCollection
                .Find(_ => true)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();

            return subscribers.Select(MapToResultDto).ToList();
        }


        public async Task<GetByIdSubscriberDto?> GetByIdAsync(string id)
        {
            var subscriber = await _subscriberCollection
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (subscriber is null)
                return null;

            return new GetByIdSubscriberDto
            {
                Id = subscriber.Id,
                FullName = subscriber.FullName,
                Email = subscriber.Email,
                DiscountCode = subscriber.DiscountCode,
                DiscountRate = subscriber.DiscountRate,
                CreatedAt = subscriber.CreatedAt,
                ExpiresAt = subscriber.ExpiresAt,
                IsUsed = subscriber.IsUsed
            };
        }


        public async Task<ResultSubscriberDto> CreateAsync(CreateSubscriberDto createSubscriberDto)
        {
            var subscriber = new Subscriber
            {
                FullName = createSubscriberDto.FullName.Trim(),
                Email = createSubscriberDto.Email.Trim().ToLowerInvariant(),

                DiscountCode = GenerateDiscountCode(),

                DiscountRate = 25,

                CreatedAt = DateTime.UtcNow,

                ExpiresAt = DateTime.UtcNow.AddDays(30),

                IsUsed = false
            };

            await _subscriberCollection.InsertOneAsync(subscriber);

            return MapToResultDto(subscriber);
        }


        public async Task DeleteAsync(string id)
        {
            await _subscriberCollection.DeleteOneAsync(x => x.Id == id);
        }


        public async Task<bool> EmailExistsAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();

            return await _subscriberCollection
                .Find(x => x.Email == normalizedEmail)
                .AnyAsync();
        }


        private static string GenerateDiscountCode()
        {
            var randomPart = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 8)
                .ToUpperInvariant();

            return $"FOOD25-{randomPart}";
        }


        private static ResultSubscriberDto MapToResultDto(Subscriber subscriber)
        {
            return new ResultSubscriberDto
            {
                Id = subscriber.Id,
                FullName = subscriber.FullName,
                Email = subscriber.Email,
                DiscountCode = subscriber.DiscountCode,
                DiscountRate = subscriber.DiscountRate,
                CreatedAt = subscriber.CreatedAt,
                ExpiresAt = subscriber.ExpiresAt,
                IsUsed = subscriber.IsUsed
            };
        }
    }
}