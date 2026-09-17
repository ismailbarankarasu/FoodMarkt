using FoodMart.Dtos.FeatureDtos;

namespace FoodMart.Services.FeatureServices
{
    public interface IFeatureService
    {
        Task<List<ResultFeatureDto>> GetAllAsync();

        Task<List<ResultFeatureDto>> GetActiveFeaturesAsync();

        Task<GetByIdFeatureDto?> GetByIdAsync(string id);

        Task CreateAsync(CreateFeatureDto createFeatureDto);

        Task UpdateAsync(UpdateFeatureDto updateFeatureDto);

        Task DeleteAsync(string id);
    }
}