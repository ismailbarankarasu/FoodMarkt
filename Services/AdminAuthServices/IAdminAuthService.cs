using FoodMart.Entities;

namespace FoodMart.Services.AdminAuthServices
{
    public interface IAdminAuthService
    {
        Task<AdminUser?> ValidateUserAsync(string email, string password);

        Task<AdminUser> CreateAdminAsync(string fullName, string email, string password);

        Task<bool> AdminExistsAsync();
    }
}