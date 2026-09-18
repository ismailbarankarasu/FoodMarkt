using FoodMart.Dtos.SubscriberDtos;

namespace FoodMart.Services.SubscriberServices
{
    public interface ISubscriberService
    {
        Task<List<ResultSubscriberDto>> GetAllAsync();

        Task<GetByIdSubscriberDto?> GetByIdAsync(string id);

        Task<ResultSubscriberDto> CreateAsync(CreateSubscriberDto createSubscriberDto);

        Task DeleteAsync(string id);

        Task<bool> EmailExistsAsync(string email);
    }
}