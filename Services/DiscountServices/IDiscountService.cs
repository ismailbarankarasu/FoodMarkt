using FoodMart.Dtos.DiscountDtos;

namespace FoodMart.Services.DiscountServices
{
    public interface IDiscountService
    {
        Task<List<ResultDiscountDto>> GetAllAsync();

        Task<List<ResultDiscountDto>> GetActiveDiscountsAsync(int count);

        Task<GetByIdDiscountDto?> GetByIdAsync(string id);

        Task CreateAsync(CreateDiscountDto createDiscountDto);

        Task UpdateAsync(UpdateDiscountDto updateDiscountDto);

        Task DeleteAsync(string id);
    }
}