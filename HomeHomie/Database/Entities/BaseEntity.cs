using MongoDB.Bson;

namespace HomeHomie.Database.Entities
{
    public abstract class BaseEntity
    {
        public ObjectId Id { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
