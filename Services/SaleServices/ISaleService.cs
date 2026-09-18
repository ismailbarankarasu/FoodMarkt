using FoodMart.Dtos.SaleDtos;

namespace FoodMart.Services.SaleServices
{
    public interface ISaleService
    {
        Task<List<ResultSaleDto>> GetAllAsync();
        Task<GetByIdSaleDto> GetByIdAsync(string id);
        Task CreateAsync(CreateSaleDto createSaleDto);
        Task UpdateAsync(UpdateSaleDto updateSaleDto);
        Task DeleteAsync(string id);
        Task<List<PopularProductDto>> GetPopularProductsAsync(int count);
    }
}
