using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FoodMart.Entities
{
    public class Sale
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductId { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    }
}