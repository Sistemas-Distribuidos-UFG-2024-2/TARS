using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationSystem.Database;

[BsonIgnoreExtraElements]
public class BaseEntity
{
    [BsonId]
    [BsonElement("_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }
}