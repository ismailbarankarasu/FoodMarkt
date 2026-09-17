using FoodMart.Dtos.CategoryDtos;

namespace FoodMart.Services.CategoryServices
{
    public interface ICategoryService
    {
        Task<List<ResultCategoryDto>> GetAllAsync();
        Task<GetByIdCategoryDto?> GetByIdAsync(string id);
        Task CreateAsync(CreateCategoryDto createCategoryDto);
        Task UpdateAsync(UpdateCategoryDto updateCategoryDto);
        Task DeleteAsync(string id);
    }
}