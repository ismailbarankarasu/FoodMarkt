namespace FoodMart.Dtos.AdminDtos
{
    public class AdminLoginDto
    {
        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
    }
}