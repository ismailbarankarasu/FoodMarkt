namespace FoodMart.Dtos.SubscriberDtos
{
    public class GetByIdSubscriberDto
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string DiscountCode { get; set; } = null!;
        public int DiscountRate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
    }
}