using MongoDB.Bson.Serialization.Attributes;
using NotificationSystem.Database;

namespace NotificationSystem.Entities;


public class Person : BaseEntity
{
    [BsonElement("name")]
    public required string Name { get; set; }
    
    [BsonElement("email")]
    public required string Email { get; set; }
}