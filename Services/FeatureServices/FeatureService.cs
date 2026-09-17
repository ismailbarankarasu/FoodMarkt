using FoodMart.Dtos.FeatureDtos;
using FoodMart.Entities;
using MongoDB.Driver;

namespace FoodMart.Services.FeatureServices
{
    public class FeatureService : IFeatureService
    {
        private readonly IMongoCollection<Feature> _featureCollection;

        public FeatureService(IMongoDatabase database)
        {
            _featureCollection = database.GetCollection<Feature>("Features");
        }

        public async Task<List<ResultFeatureDto>> GetAllAsync()
        {
            var features = await _featureCollection
                .Find(_ => true)
                .SortBy(x => x.Order)
                .ToListAsync();

            return MapFeatures(features);
        }

        public async Task<List<ResultFeatureDto>> GetActiveFeaturesAsync()
        {
            var features = await _featureCollection
                .Find(x => x.IsActive)
                .SortBy(x => x.Order)
                .ToListAsync();

            return MapFeatures(features);
        }

        public async Task<GetByIdFeatureDto?> GetByIdAsync(string id)
        {
            var feature = await _featureCollection
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (feature is null)
                return null;

            return new GetByIdFeatureDto
            {
                Id = feature.Id,
                Title = feature.Title,
                Description = feature.Description,
                ImageUrl = feature.ImageUrl,
                ButtonText = feature.ButtonText,
                ButtonUrl = feature.ButtonUrl,
                Order = feature.Order,
                IsActive = feature.IsActive
            };
        }

        public async Task CreateAsync(CreateFeatureDto dto)
        {
            var feature = new Feature
            {
                Title = dto.Title,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                ButtonText = dto.ButtonText,
                ButtonUrl = dto.ButtonUrl,
                Order = dto.Order,
                IsActive = dto.IsActive
            };

            await _featureCollection.InsertOneAsync(feature);
        }

        public async Task UpdateAsync(UpdateFeatureDto dto)
        {
            var feature = new Feature
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                ButtonText = dto.ButtonText,
                ButtonUrl = dto.ButtonUrl,
                Order = dto.Order,
                IsActive = dto.IsActive
            };

            await _featureCollection.ReplaceOneAsync(
                x => x.Id == dto.Id,
                feature);
        }

        public async Task DeleteAsync(string id)
        {
            await _featureCollection.DeleteOneAsync(x => x.Id == id);
        }

        private static List<ResultFeatureDto> MapFeatures(List<Feature> features)
        {
            return features.Select(x => new ResultFeatureDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                ButtonText = x.ButtonText,
                ButtonUrl = x.ButtonUrl,
                Order = x.Order,
                IsActive = x.IsActive
            }).ToList();
        }
    }
}