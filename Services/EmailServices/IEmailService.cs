namespace FoodMart.Services.EmailServices
{
    public interface IEmailService
    {
        Task SendDiscountCodeAsync(string fullName, string email, string discountCode, int discountRate, DateTime expiresAt);
    }
}