using FoodMart.Dtos.ProductDtos;

namespace FoodMart.Services.ProductServices
{
    public interface IProductService
    {
        Task<List<ResultProductDto>> GetAllAsync();

        Task<GetByIdProductDto?> GetByIdAsync(string id);

        Task CreateAsync(CreateProductDto createProductDto);

        Task UpdateAsync(UpdateProductDto updateProductDto);

        Task DeleteAsync(string id);

        Task<List<ResultProductDto>> GetLatestProductsAsync(int count);

        Task<List<ResultProductDto>> GetProductsByCategoryAsync(
            string categoryId);

        Task<List<ResultProductDto>> SearchProductsAsync(
            string search);
    }
}