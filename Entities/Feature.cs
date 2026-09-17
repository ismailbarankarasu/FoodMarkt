using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FoodMart.Entities
{
    public class Feature
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string ImageUrl { get; set; } = null!;

        public string ButtonText { get; set; } = null!;

        public string ButtonUrl { get; set; } = null!;

        public int Order { get; set; }

        public bool IsActive { get; set; } = true;
    }
}