using FoodMart.Dtos.DashboardDtos;

namespace FoodMart.Services.DashboardServices
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync();
    }
}